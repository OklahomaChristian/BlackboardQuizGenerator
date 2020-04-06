using System.Collections.Generic;

namespace QuizParseLibrary
{
    /// <summary>
    /// Summary description for Question
    /// </summary>
    public class Question
    {
        private string m_strRawText;            //The unfiltered question text (as written by the user)
        private QuestionType m_questionType;    //The type of question
        private int m_nQuestionNumber;          //The question number
        private string m_strQuestionText;       //The question text
        private List<Answer> m_lstAnswers;      //The question answers

        private string m_strErrors;            //The question errors - if any

        /// <summary>
        /// The unfiltered question text, as written by the user
        /// </summary>
        public string RawText
        {
            get { return m_strRawText; }
        }

        /// <summary>
        /// The type of question - Enumerated type QuestionType
        /// </summary>
        public QuestionType Type
        {
            get { return m_questionType; }
            set { m_questionType = value; }
        }

        /// <summary>
        /// The question text
        /// </summary>
        public string QuestionText
        {
            get { return m_strQuestionText; }
            set { m_strQuestionText = value; }
        }

        /// <summary>
        /// The list of answers for this question
        /// </summary>
        public List<Answer> Answers
        {
            get { return m_lstAnswers; }
        }

        /// <summary>
        /// An error message holder - used by the parser
        /// </summary>
        public string ErrorMessage
        {
            get { return m_strErrors; }
            set { m_strErrors = value; }
        }

        /// <summary>
        /// The question number, as parsed
        /// </summary>
        public int QuestionNumber
        {
            get { return m_nQuestionNumber; }
            set { m_nQuestionNumber = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Question( string strRawText = "" )
        {
            m_strRawText = strRawText;
            m_questionType = QuestionType.QT_UNKNOWN;
            m_strQuestionText = "";
            m_lstAnswers = new List<Answer>();
            m_strErrors = "";
            m_nQuestionNumber = -1;
        }

        /// <summary>
        /// Check if this question is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return m_questionType != QuestionType.QT_UNKNOWN;
        }

        /// <summary>
        /// Add an answer to the question
        /// </summary>
        /// <param name="answer"></param>
        public void AddAnswer(Answer answer)
        {
            m_lstAnswers.Add(answer);
        }

        /// <summary>
        /// Get the type of question, as a string
        /// </summary>
        /// <returns></returns>
        public string GetQuestionTypeString()
        {
            string strType = "";

            switch (m_questionType)
            {
                case QuestionType.QT_ESSAY:
                    strType = "Essay";
                    break;
                case QuestionType.QT_FILL_IN_THE_BLANK:
                    strType = "Fill in the blank";
                    break;
                case QuestionType.QT_MATCHING:
                    strType = "Matching";
                    break;
                case QuestionType.QT_MULTIPLE_ANSWER:
                    strType = "Multiple answer";
                    break;
                case QuestionType.QT_MULTIPLE_CHOICE:
                    strType = "Multiple Choice";
                    break;
                case QuestionType.QT_TRUE_OR_FALSE:
                    strType = "True or False";
                    break;
                case QuestionType.QT_UNKNOWN:
                    strType = "Unknown/Invalid";      
                    break;
                case QuestionType.QT_LEGACY_HEADER:
                    strType = "Question type header";
                    break;
            }

            return strType;
        }

        /// <summary>
        /// Get the string to import into Blackboard's test questions
        /// </summary>
        /// <returns></returns>
        public string GetBlackboardTestQuestionString()
        {
            string strQuestion = "";

            switch (m_questionType)
            {
                case QuestionType.QT_ESSAY:
                    strQuestion = "ESS\t" + m_strQuestionText;
                    break;
                case QuestionType.QT_FILL_IN_THE_BLANK:
                    strQuestion = "FIB\t" + m_strQuestionText;
                    foreach (Answer answer in m_lstAnswers)
                        strQuestion += "\t" + answer.Text;
                    break;
                case QuestionType.QT_MULTIPLE_ANSWER:
                    strQuestion = "MA\t" + m_strQuestionText;
                    foreach (Answer answer in m_lstAnswers)
                        strQuestion += "\t" + answer.Text + "\t" + (answer.IsCorrect ? "Correct" : "Incorrect");
                    break;
                case QuestionType.QT_MULTIPLE_CHOICE:
                    strQuestion = "MC\t" + m_strQuestionText;
                    foreach (Answer answer in m_lstAnswers)
                        strQuestion += "\t" + answer.Text + "\t" + (answer.IsCorrect ? "Correct" : "Incorrect");
                    break;
                case QuestionType.QT_TRUE_OR_FALSE:
                    strQuestion = "TF\t" + m_strQuestionText;
                    foreach (Answer answer in m_lstAnswers)
                    {
                        if (answer.IsCorrect)
                        {
                            strQuestion += "\t" + answer.Text;
                        }
                    }
                    break;
                case QuestionType.QT_MATCHING:
                    strQuestion = "MAT ";
                    return "ERROR";
                case QuestionType.QT_UNKNOWN:
                    return "ERROR";
            }


            return strQuestion;
        }

        public string GetBrightspaceTestQuestionString()
        {
            string strQuestion = "";

            switch (m_questionType)
            {
                case QuestionType.QT_ESSAY:
                    strQuestion = "NewQuestion,WR,\n";
                    strQuestion = strQuestion + "QuestionText," + m_strQuestionText + ",\n";
                    strQuestion = strQuestion + "AnswerKey," + " " + ",\n";
                    break;
                case QuestionType.QT_FILL_IN_THE_BLANK:
                    strQuestion = "NewQuestion,SA,\n";
                    strQuestion = strQuestion + "QuestionText," + m_strQuestionText + ",\n";
                    foreach (Answer answer in m_lstAnswers)
                    {
                        if (answer.IsCorrect)
                            strQuestion += "Answer,100," + answer.Text + ",\n";
                    }
                    break;

                case QuestionType.QT_MULTIPLE_ANSWER:
                    strQuestion = "NewQuestion,MS,\n";
                    strQuestion = strQuestion + "QuestionText," + m_strQuestionText + ",\n";
                    foreach (Answer answer in m_lstAnswers)
                    {
                        if (answer.IsCorrect)
                            strQuestion += "Option,1," + answer.Text + ",\n";
                        else
                            strQuestion += "Option,0," + answer.Text + ",\n";
                    }
                    break;
                case QuestionType.QT_MULTIPLE_CHOICE:
                    strQuestion = "NewQuestion,MC,\n";
                    strQuestion = strQuestion + "QuestionText," + m_strQuestionText + ",\n";
                    foreach (Answer answer in m_lstAnswers)
                    {
                        if (answer.IsCorrect)
                            strQuestion += "Option,100," + answer.Text + ",\n";
                        else
                            strQuestion += "Option,0," + answer.Text + ",\n";
                    }

                    break;
                case QuestionType.QT_TRUE_OR_FALSE:
                    strQuestion = "NewQuestion,TF,\n";
                    strQuestion = strQuestion + "QuestionText," + m_strQuestionText + ",\n";
                    foreach (Answer answer in m_lstAnswers)
                    {
                        if (answer.IsCorrect)
                            strQuestion += "TRUE,100," + answer.Text + ",\n";
                        else
                            strQuestion += "FALSE,0," + answer.Text + ",\n";
                    }
                    break;
                case QuestionType.QT_MATCHING:
                    //strQuestion = "MAT ";
                    return "ERROR";
                case QuestionType.QT_UNKNOWN:
                    return "ERROR";
            }


            return strQuestion;
        }
    }
}