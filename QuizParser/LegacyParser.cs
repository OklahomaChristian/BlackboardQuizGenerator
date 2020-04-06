
namespace QuizParseLibrary
{
    class LegacyParser
    {
        //The question type identifiers
        public static readonly string[] LEGACY_QUESTION_TYPES = { "MC", "MA", "TF", "BL", "ES" };
        private string m_strCurrentQuestionBlock = "";

        public void ClearActiveQuestionBlock()
        {
            m_strCurrentQuestionBlock = "";
        }

        /// <summary>
        /// Parse a question using the legacy formats (from the old test generator)
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public Question ParseQuestion(Question question)
        {
            string[] lines = question.RawText.Split('\n');

            if (lines.Length > 0)
            {
                //Begin parsing question data at index 1 if there was a block identifier at the start of the question
                int nStartIndex = 1;
                QuestionType questionType = this.GetType(lines[0].ToUpper());

                //If the first line isn't defining a legacy type, check if we're inside a current block
                if (QuestionType.QT_UNKNOWN == questionType)
                {
                    questionType = this.GetType(m_strCurrentQuestionBlock);
                    nStartIndex = 0;
                }
                else
                    m_strCurrentQuestionBlock = lines[0].ToUpper(); //We've entered a new question block

                //Sometimes there are orphan block headers
                if (lines.Length > 1)
                {
                    question.Type = questionType;
                    question.QuestionText = lines[nStartIndex];

                    //Gather the answers
                    int nPreAnswerLineCount = nStartIndex + 1;
                    string[] strAnswers = new string[lines.Length - nPreAnswerLineCount];

                    for (int i = nPreAnswerLineCount; i < lines.Length; ++i)
                        strAnswers[i - nPreAnswerLineCount] = lines[i];

                    this.ParseAnswers(strAnswers, question);
                }
                else
                    question.Type = QuestionType.QT_LEGACY_HEADER;
            }

            return question;
        }
        
        /// <summary>
        /// Get the enumerated question type from the legacy string identifiers
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        private QuestionType GetType(string strType)
        {
            QuestionType type = QuestionType.QT_UNKNOWN;
           
            switch (strType)
            {
                case "MC":
                    type = QuestionType.QT_MULTIPLE_CHOICE;
                    break;
                case "MA":
                    type = QuestionType.QT_MULTIPLE_ANSWER;
                    break;
                case "TF":
                    type = QuestionType.QT_TRUE_OR_FALSE;
                    break;
                case "ES":
                    type = QuestionType.QT_ESSAY;
                    break;
                case "BL":
                    type = QuestionType.QT_FILL_IN_THE_BLANK;
                    break;
                default:
                    type = QuestionType.QT_UNKNOWN;
                    break;
            }

            return type;
        }

        /// <summary>
        /// Parse the answers for a particular question
        /// </summary>
        /// <param name="strAnswers"></param>
        /// <param name="question"></param>
        private void ParseAnswers(string[] strAnswers, Question question)
        {
            switch (question.Type)
            {
                case QuestionType.QT_ESSAY:
                {
                    if (strAnswers.Length != 0)
                    {
                        question.Type = QuestionType.QT_UNKNOWN;
                        question.ErrorMessage = "Essay questions must be written on one line, and can not have any answers";
                    }
                    break;
                }
                case QuestionType.QT_FILL_IN_THE_BLANK:
                {
                    if (strAnswers.Length == 0)
                    {
                        question.Type = QuestionType.QT_UNKNOWN;
                        question.ErrorMessage = "Fill in the blank questions must have at least one answer";
                    }
                    else
                    {
                        foreach (string strAnswer in strAnswers)
                        {
                            Answer answer = new Answer();
                            answer.Text = strAnswer;
                            answer.IsCorrect = true;

                            question.AddAnswer(answer);
                        }
                    }

                    break;
                }
                case QuestionType.QT_MULTIPLE_ANSWER:
                {
                    if (strAnswers.Length == 0)
                    {
                        question.Type = QuestionType.QT_UNKNOWN;
                        question.ErrorMessage = "Multiple answer questions must have at least one answer";
                    }
                    else
                    {
                        int nCorrectAnswers = 0;

                        foreach (string strAnswer in strAnswers)
                        {
                            Answer answer = new Answer();

                            if (strAnswer[0] == '*')
                            {
                                answer.IsCorrect = true;
                                answer.Text = strAnswer.Substring(1);

                                nCorrectAnswers++;
                            }
                            else
                                answer.Text = strAnswer;

                            question.AddAnswer(answer);
                        }

                        if (nCorrectAnswers < 1)
                        {
                            question.Type = QuestionType.QT_UNKNOWN;
                            question.ErrorMessage = "No correct answers found";
                        }
                    }

                    break;
                }
                case QuestionType.QT_MULTIPLE_CHOICE:
                {
                    if (strAnswers.Length == 0)
                    {
                        question.Type = QuestionType.QT_UNKNOWN;
                        question.ErrorMessage = "Multiple Choice questions must have at least one answer";
                    }
                    else
                    {
                        int nCorrectAnswers = 0;

                        foreach (string strAnswer in strAnswers)
                        {
                            Answer answer = new Answer();

                            if (strAnswer[0] == '*')
                            {
                                answer.IsCorrect = true;
                                answer.Text = strAnswer.Substring(1);

                                nCorrectAnswers++;
                            }
                            else
                                answer.Text = strAnswer;

                            question.AddAnswer(answer);
                        }

                        if (nCorrectAnswers < 1)
                        {
                            question.Type = QuestionType.QT_UNKNOWN;
                            question.ErrorMessage = "No correct answers found";
                        }
                        else if (nCorrectAnswers > 1)
                        {
                            question.Type = QuestionType.QT_UNKNOWN;
                            question.ErrorMessage = "Multiple Choice questions can only have 1 correct answer";
                        }
                    }
                    
                    break;
                }
                case QuestionType.QT_TRUE_OR_FALSE:
                {
                    question.Type = QuestionType.QT_UNKNOWN;
                    question.ErrorMessage = "True or false questions must have 1 answer (T or F)";

                    if (strAnswers.Length == 1)
                    {
                        //Only grab the first character (t/f)
                        string strAnswer = strAnswers[0].ToLower()[0].ToString();

                        if (strAnswer == "t" || strAnswer == "f")
                        {
                            Answer answerTrue = new Answer();
                            answerTrue.Text = "true";
                            answerTrue.IsCorrect = (strAnswer == "t");

                            Answer answerFalse = new Answer();
                            answerFalse.Text = "false";
                            answerFalse.IsCorrect = (strAnswer == "f");

                            question.AddAnswer(answerTrue);
                            question.AddAnswer(answerFalse);
                            question.Type = QuestionType.QT_TRUE_OR_FALSE;
                        }
                        else
                        {
                            question.ErrorMessage = "True and False are the only valid answers for this question type.";
                        }
                    }

                    break;
                }
            }
        }

    }
}
