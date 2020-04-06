using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace QuizParseLibrary
{
    /// <summary>
    /// Summary description for QuizParser
    /// </summary>
    public class QuizParser
    {
        //Regular expressions to check the Question line against
        private Regex m_rxDefaultFirstLine;
        private Regex m_rxBlankFirstLine;
        private Regex m_rxMatchFirstLine;

        //Regular expressions to check the answer lines against
        private Regex m_rxRegularAnswer;
        private Regex m_rxTrueOrFalseAnswer;

        //These identifiers are used within the regular expressions
        private const string QUESTION_NUMBER = "questionNumber";
        private const string QUESTION_TEXT = "questionText";
        private const string ANSWER_NUMBER = "answerNumber";
        private const string ANSWER_SEPERATOR = "answerSeperator";
        private const string ANSWER_TEXT = "answerText";
        private const string IS_CORRECT = "isCorrect";
        private const string TRUE_FALSE = "trueFalse";

        //For managing blocks of legacy questions
        private LegacyParser m_legacyParser;  

        /// <summary>
        /// Constructor
        /// </summary>
        public QuizParser()
        {
            ////////////////////////////////
            //Questions - NOTE: Questions must have numeric identifiers
            //First line expression: No string of any length, followed by at least 1 number with either a ) or a period.  Followed by some text
            m_rxDefaultFirstLine = new Regex(@"^\s*(?<" + QUESTION_NUMBER + @">\d+)[\)\.]\s*(?<" + QUESTION_TEXT + @">.+)$");

            //First line of a fill in the blank question is a little different
            m_rxBlankFirstLine = new Regex(@"^\s*blank (?<" + QUESTION_NUMBER + @">\d+)[\)\.]\s*(?<" + QUESTION_TEXT + @">.+)$", RegexOptions.IgnoreCase);

            //First line of match excercise is also a little different
            m_rxMatchFirstLine = new Regex(@"^\s*match (?<" + QUESTION_NUMBER + @">\d+)[\)\.]\s*(?<" + QUESTION_TEXT + @">.+)$", RegexOptions.IgnoreCase);

            ////////////////////////////////////////////////
            //Answers - NOTE: Answers with identifiers must use letters
            //m_rxRegularAnswer       = new Regex( @"^\s*(?<"+IS_CORRECT+@">\*)?(?<"+ANSWER_NUMBER+@">(\d+|\w))(?<"+ANSWER_SEPERATOR+@">[\)\.])\s*(?<"+ANSWER_TEXT+@">.+)$" );
            m_rxRegularAnswer = new Regex(@"^\s*(?<" + IS_CORRECT + @">\*)?(?<" + ANSWER_NUMBER + @">[a-zA-Z])(?<" + ANSWER_SEPERATOR + @">[\)\.])\s*(?<" + ANSWER_TEXT + @">.+)$");
            m_rxTrueOrFalseAnswer = new Regex(@"^\s*(?<" + TRUE_FALSE + @">t|true|f|false)\s*$", RegexOptions.IgnoreCase);

            m_legacyParser = new LegacyParser();
        }

        /// <summary>
        /// Generate a quiz from a string
        /// </summary>
        /// <param name="strText">the string containing the quiz questions</param>
        /// <param name="quiz">the quiz object that is generated</param>
        /// <returns>True if generation is successful, false otherwise</returns>
        public bool GenerateQuiz(string strText, out Quiz quiz)
        {
            //LegacyPreParser legacyPreParser = new LegacyPreParser();

            strText = StringUtils.RemoveSpecialCharacters(strText);
            //strText = legacyPreParser.ConvertOldStyleQuestions(strText);

            bool bSuccess = true;
            quiz = new Quiz();

            //Seperate the input into lines
            string[] strQuestions = StringUtils.SeparateParagraphs(strText);

            strQuestions = this.GatherOrphanAnswers(strQuestions);

            for (int i = 0; i < strQuestions.Length; ++i)
            {
                string strQuestion = strQuestions[i];

                Question question = this.ParseQuestion(strQuestion);

                if (question.Type == QuestionType.QT_LEGACY_HEADER)
                {
                    //Append the legacy header to the next question.  If there is one
                    if ((i + 1) < strQuestions.Length)
                    {
                        strQuestions[i + 1] = question.RawText + "\n" + strQuestions[i + 1];
                        continue; //Don't add this as a question
                    }
                    else
                    {
                        question.Type = QuestionType.QT_UNKNOWN;
                        question.ErrorMessage = "This question block header appears to have no questions";
                    }

                }


                if (question.Type == QuestionType.QT_UNKNOWN)
                {
                    //Couldn't determine question type - there was a problem
                    bSuccess = false;
                }

                //Adding the question
                quiz.AddQuestion(question);
            }

            return bSuccess;
        }

        /// <summary>
        /// Combine orphan question paragraphs with orphan answer paragraphs
        /// </summary>
        /// <param name="strInputParagraphs"></param>
        /// <returns></returns>
        private string[] GatherOrphanAnswers(string[] strInputParagraphs)
        {
            //This is only relevant if there are more than 1 possible questions
            if (strInputParagraphs.Length < 2)
                return strInputParagraphs;

            List<string> lstQuestions = new List<string>();
            lstQuestions.Add(strInputParagraphs[0]);

            //Determine if each paragraph is an answer group, and if so.. check if the previous paragraph was a question group
            for (int i = 1; i < strInputParagraphs.Length; ++i)
            {
                //Check if the current block is filled with answers
                bool bIsAnswerBlock = this.CheckForAnswerBlock(strInputParagraphs[i]);
                bool bIsQuestionBlock = this.CheckForQuestionBlock(strInputParagraphs[i - 1]);

                //Combine the answer block with the previous question block
                if (bIsAnswerBlock && bIsQuestionBlock)
                    lstQuestions[lstQuestions.Count - 1] += "\n" + strInputParagraphs[i];
                else
                    lstQuestions.Add(strInputParagraphs[i]);
            }

            return lstQuestions.ToArray();
        }

        /// <summary>
        /// Check if a paragraph contains only answers
        /// </summary>
        /// <param name="strParagraph">the paragraph to check</param>
        /// <returns>true if the paragraph contains only answers</returns>
        private bool CheckForAnswerBlock(string strParagraph)
        {
            //validate input
            if (strParagraph.Length == 0)
                return false;

            string[] lines = strParagraph.Split('\n');

            //Eliminate true or false answers first - There is only one line, and it reads true or false
            if (lines.Length == 1)
            {
                if (m_rxTrueOrFalseAnswer.Match(lines[0]).Success)
                    return true;
            }

            //Otherwise check for a well formed regular answer block
            foreach (string line in lines)
            {
                Match matchAnswer = m_rxRegularAnswer.Match(line);

                //if a line doesn't match the answer pattern - this is not an answer block
                if (false == matchAnswer.Success)
                    return false;

                //Answer may only use letters as identifiers
                string strID = matchAnswer.Groups[ANSWER_NUMBER].Value.ToString();
                int nID = 0;
                if (int.TryParse(strID, out nID))
                    return false;

            }

            return true;
        }

        /// <summary>
        /// Check if a paragraph contains only a question
        /// </summary>
        /// <param name="strParagraph"></param>
        /// <returns></returns>
        private bool CheckForQuestionBlock(string strParagraph)
        {
            string[] lines = strParagraph.Split('\n');

            if (lines.Length == 1)
            {
                Match matchDefaultFirstLine = m_rxDefaultFirstLine.Match(lines[0]);
                Match matchBlankFirstLine = m_rxBlankFirstLine.Match(lines[0]);
                Match matchMatchFirstLine = m_rxMatchFirstLine.Match(lines[0]);

                if (matchDefaultFirstLine.Success || matchBlankFirstLine.Success || matchMatchFirstLine.Success)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Parse the individual questions
        /// </summary>
        /// <param name="strQuestion"></param>
        /// <returns></returns>
        private Question ParseQuestion(string strQuestion)
        {
            Question question = new Question(strQuestion);

            string[] strLines = strQuestion.Split('\n');

            //Check the first line to determine if it's a question line
            Match matchDefaultFirstLine = m_rxDefaultFirstLine.Match(strLines[0]);
            Match matchBlankFirstLine = m_rxBlankFirstLine.Match(strLines[0]);
            Match matchMatchFirstLine = m_rxMatchFirstLine.Match(strLines[0]);

            if (matchDefaultFirstLine.Success)
            {
                //End the legacy question block - we've found a modern format question
                m_legacyParser.ClearActiveQuestionBlock();

                //Get the question values from the expression
                string strQuestionNumber = matchDefaultFirstLine.Groups[QUESTION_NUMBER].Value.Trim();

                question.QuestionText = matchDefaultFirstLine.Groups[QUESTION_TEXT].Value.Trim();
                question.QuestionNumber = int.Parse(strQuestionNumber);

                //This question was one of the types that does not have a string qualifier at the start of the line
                int nNumAnswers = strLines.Length - 1;

                if (nNumAnswers == 0)
                {
                    //Questions without answers are considered essay questions
                    question.Type = QuestionType.QT_ESSAY;
                }
                else if (nNumAnswers == 1)
                {
                    //Only a True or false question *should* have a single answer.  Any other 1 answer case is an error and should be reported
                    this.ParseTrueOrFalseQuestion(question, strLines[1]);
                }
                else
                {
                    //Count the number of correct answers - 1 == multiple choice question, more than one == multiple answer
                    int nCorrectAnswers = 0;

                    for (int i = 1; i < strLines.Length; ++i)
                    {
                        Match matchAnswer = m_rxRegularAnswer.Match(strLines[i]);

                        if (matchAnswer.Success)
                        {
                            bool bIsCorrect = matchAnswer.Groups[IS_CORRECT].Value.Length > 0;

                            if (bIsCorrect)
                                ++nCorrectAnswers;

                            //Add the answer to the question
                            Answer answer = new Answer();
                            answer.AnswerID = matchAnswer.Groups[ANSWER_NUMBER].Value.Trim();
                            answer.IsCorrect = bIsCorrect;
                            answer.Text = matchAnswer.Groups[ANSWER_TEXT].Value.Trim();

                            question.AddAnswer(answer);
                        }
                        else
                        {
                            //Quit on invalid answer
                            question.Type = QuestionType.QT_UNKNOWN;
                            question.ErrorMessage = "This question has an invalid answer: " + strLines[i];
                            return question;
                        }
                    }

                    if (nCorrectAnswers == 0)
                        question.ErrorMessage = "No correct answers were found for this question.  Please place an asterix in front of the correct answer.";
                    else if (nCorrectAnswers == 1)
                        question.Type = QuestionType.QT_MULTIPLE_CHOICE;
                    else
                        question.Type = QuestionType.QT_MULTIPLE_ANSWER;
                }
            }
            else if (matchBlankFirstLine.Success)
            {
                //End the legacy question block - we've found a modern format question
                m_legacyParser.ClearActiveQuestionBlock();

                //Get the question values from the expression
                string strQuestionNumber = matchBlankFirstLine.Groups[QUESTION_NUMBER].Value.Trim();
                question.QuestionText = matchBlankFirstLine.Groups[QUESTION_TEXT].Value.Trim();
                question.QuestionNumber = int.Parse(strQuestionNumber);

                question.Type = QuestionType.QT_FILL_IN_THE_BLANK;

                for (int i = 1; i < strLines.Length; ++i)
                {
                    Match matchAnswer = m_rxRegularAnswer.Match(strLines[i]);

                    if (matchAnswer.Success)
                    {
                        //Add the answer to the question - all 
                        Answer answer = new Answer();
                        answer.AnswerID = matchAnswer.Groups[ANSWER_NUMBER].Value.Trim();
                        answer.IsCorrect = true;
                        answer.Text = matchAnswer.Groups[ANSWER_TEXT].Value.Trim();

                        question.AddAnswer(answer);
                    }
                    else
                    {
                        //Quit on invalid answer
                        question.Type = QuestionType.QT_UNKNOWN;
                        question.ErrorMessage = "This question has an invalid answer: " + strLines[i] + ". Note that all answers should begin with a letter followed by a period or round bracket.";
                        return question;
                    }
                }
            }
            else if (matchMatchFirstLine.Success)
            {
                //End the legacy question block - we've found a modern format question
                m_legacyParser.ClearActiveQuestionBlock();

                /* Not supporting match questions at this time
                //question.Type = QuestionType.QT_MATCHING;
                 */
                question.Type = QuestionType.QT_UNKNOWN;
                question.ErrorMessage = "Matching questions are not supported at this time";
            }
            else
            {
                question.Type = QuestionType.QT_UNKNOWN; //This isn't a question
                question.ErrorMessage = "This question could not be resolved to any known type.  Check the help pages for proper question formatting.";
            }

            /////////////////////////////////
            //Support the legacy format
            //
            //If the question type couldn't be determined, check if it's an old format question
            if (QuestionType.QT_UNKNOWN == question.Type)
            {
                question = m_legacyParser.ParseQuestion(question);
            }

            return question;
        }

        /// <summary>
        /// Check if a question is true or false
        /// </summary>
        /// <param name="question"></param>
        /// <param name="strAnswer"></param>
        private bool ParseTrueOrFalseQuestion(Question question, string strAnswer)
        {
            bool bIsTrueOrFalseQuestion = false;

            //Check for a true or false question
            Match matchTrueOrFalse = m_rxTrueOrFalseAnswer.Match(strAnswer);

            if (matchTrueOrFalse.Success)
            {
                question.Type = QuestionType.QT_TRUE_OR_FALSE;

                string strTrueOrFalseAnswer = matchTrueOrFalse.Groups[TRUE_FALSE].Value.Trim().ToLower();

                //add the true or false answer - I admit, this is kind of strange
                Answer answerTrue = new Answer();
                answerTrue.Text = "true";
                answerTrue.IsCorrect = (strTrueOrFalseAnswer == "true");

                Answer answerFalse = new Answer();
                answerFalse.Text = "false";
                answerFalse.IsCorrect = (strTrueOrFalseAnswer == "false");

                question.AddAnswer(answerTrue);
                question.AddAnswer(answerFalse);

                //Success!
                bIsTrueOrFalseQuestion = true;
            }
            else
                question.ErrorMessage = "Only one answer was found and it was not true or false";

            return bIsTrueOrFalseQuestion;
        }
    }
}