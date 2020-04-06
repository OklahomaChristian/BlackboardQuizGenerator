namespace QuizParseLibrary
{
    /// <summary>
    /// Summary description for Answer
    /// </summary>
    public class Answer
    {
        private string  m_strText;
        private string m_strAnswerID;
        private bool m_bIsCorrect;

        /// <summary>
        /// The identifier of the answer ( ex. a) or b.)
        /// </summary>
        public string AnswerID
        {
            get { return m_strAnswerID; }
            set { m_strAnswerID = value; }
        }

        /// <summary>
        /// The text of the answer
        /// </summary>
        public string Text
        {
            get { return m_strText; }
            set { m_strText = value; }
        }

        /// <summary>
        /// is this answer the correct one?
        /// </summary>
        public bool IsCorrect
        {
            get { return m_bIsCorrect; }
            set { m_bIsCorrect = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Answer()
        {
            m_strAnswerID = "";
            m_strText = "";
            m_bIsCorrect = false;
        }
    }
}