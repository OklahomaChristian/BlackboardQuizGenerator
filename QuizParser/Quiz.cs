using System.Collections.Generic;
using System;
using System.Xml;
using System.Text;

namespace QuizParseLibrary
{
    /// <summary>
    /// Summary description for Quiz
    /// </summary>
    public class Quiz
    {
        //Quiz data
        private List<Question> m_lstQuestions;
        private string m_strQuizName;

        public List<Question> Questions
        {
            get { return m_lstQuestions; }
        }

        public string QuizName
        {
            get { return m_strQuizName; }
            set { m_strQuizName = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Quiz()
        {
            m_strQuizName = "bbQuiz";
            m_lstQuestions = new List<Question>();
        }

        public void AddQuestion(Question question)
        {
            m_lstQuestions.Add(question);
        }

        /// <summary>
        /// Get the Blackboard pool XML String
        /// </summary>
        /// <returns></returns>
        public string GetBlackboardPoolXMLString()
        {
            StringWriterWithEncoding sw = new StringWriterWithEncoding(Encoding.UTF8);
            XmlTextWriter xml = new XmlTextWriter(sw);
            xml.Formatting = Formatting.Indented;
            xml.WriteStartDocument();

            this.WriteQuizHeader(xml);

            foreach (Question question in m_lstQuestions)
            {
                if (question.IsValid())
                    this.WriteQuestionXML(xml, question);
            }

            xml.WriteEndDocument();

            xml.Flush();
            xml.Close();

            return sw.ToString();
        }

        public string GetBlackboardTestQuestionString()
        {
            string strQuizQuestions = "";

            foreach (Question question in m_lstQuestions)
                strQuizQuestions += question.GetBlackboardTestQuestionString() + "\n";

            return strQuizQuestions;
        }

        public string GetBrightspaceTestQuestionString()
        {
            string strQuizQuestions = "";

            foreach (Question question in m_lstQuestions)
                strQuizQuestions += question.GetBrightspaceTestQuestionString() + "\n";

            return strQuizQuestions;
        }

        /// <summary>
        /// Write the quiz header information
        /// </summary>
        /// <param name="xml"></param>
        private void WriteQuizHeader(XmlTextWriter xml)
        {
            xml.WriteStartElement("POOL");

            //Course ID
            xml.WriteStartElement("COURSEID");
            xml.WriteAttributeString("value", "IMPORT");
            xml.WriteEndElement();

            //Title
            xml.WriteStartElement("TITLE");
            xml.WriteAttributeString("value", m_strQuizName.Length > 0 ? m_strQuizName : "A Blackboard Quiz");
            xml.WriteEndElement();

            //Decription
            xml.WriteStartElement("DESCRIPTION");
            xml.WriteStartElement("TEXT");
            xml.WriteString("Created by the Blackboard Quiz Generator");
            xml.WriteEndElement();
            xml.WriteEndElement();

            this.WriteDates(xml);

            //Question list
            xml.WriteStartElement("QUESTIONLIST");

            //Write all the questions by ID
            int nQuestionNumber = 0;
            foreach (Question question in m_lstQuestions)
            {
                string strClassName = this.GetQuestionClass(question);

                if (strClassName.Length > 0)
                {
                    string strQuestionID = "q" + (++nQuestionNumber);

                    //This question number will be used again to link the question ID to the class (in WriteQuestionXML)
                    question.QuestionNumber = nQuestionNumber;

                    xml.WriteStartElement("QUESTION");
                    xml.WriteAttributeString("id", strQuestionID);
                    xml.WriteAttributeString("class", strClassName);
                    xml.WriteEndElement();
                }
            }

            xml.WriteEndElement();
        }

        /// <summary>
        /// Output the XML for an individual question - fasten your seatbelts, this gets a little wonky
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="question"></param>
        private void WriteQuestionXML(XmlTextWriter xml, Question question)
        {
            string strClassName = this.GetQuestionClass(question);
            string strQuestionID = "q" + question.QuestionNumber;

            //Header
            xml.WriteStartElement(strClassName);
            xml.WriteAttributeString("id", strQuestionID);

            this.WriteDates(xml);

            //Body
            xml.WriteStartElement("BODY");
            xml.WriteStartElement("TEXT");
            xml.WriteString(question.QuestionText);
            xml.WriteEndElement();

            //Flags -- These are the same for each type of question it seems
            xml.WriteStartElement("FLAGS");
            xml.WriteAttributeString("value", "true");
            xml.WriteStartElement("ISHTML");
            xml.WriteAttributeString("value", "true");
            xml.WriteEndElement();

            xml.WriteStartElement("ISNEWLINELITERAL");
            xml.WriteEndElement();
            xml.WriteEndElement(); //Flags
            xml.WriteEndElement(); //Body

            //Output the answers
            int nAnswerNumber = 0;
            List<string> lstCorrectAnswerIDs = new List<string>();
            foreach (Answer answer in question.Answers)
            {
                ++nAnswerNumber;
                string strAnswerID = strQuestionID + "_" + "a" + nAnswerNumber;

                //Store the correct answers
                if (answer.IsCorrect)
                    lstCorrectAnswerIDs.Add(strAnswerID);

                xml.WriteStartElement("ANSWER");
                xml.WriteAttributeString("id", strAnswerID);
                xml.WriteAttributeString("position", nAnswerNumber.ToString());

                this.WriteDates(xml);

                xml.WriteStartElement("TEXT");
                xml.WriteString(answer.Text);
                xml.WriteEndElement();

                xml.WriteEndElement(); //Answer
            }

            //Output the gradable area for questions that support it -- NOTE: This is handled differently by matching questions
            if (question.Type == QuestionType.QT_MULTIPLE_ANSWER ||
                question.Type == QuestionType.QT_MULTIPLE_CHOICE ||
                question.Type == QuestionType.QT_TRUE_OR_FALSE ||
                question.Type == QuestionType.QT_FILL_IN_THE_BLANK)
            {
                xml.WriteStartElement("GRADABLE");
                this.WriteFeedback(xml);

                //The correct answer section is not used for fill in the blank questions
                if (QuestionType.QT_FILL_IN_THE_BLANK != question.Type)
                {
                    foreach (string strCorrectAnswerId in lstCorrectAnswerIDs)
                    {
                        xml.WriteStartElement("CORRECTANSWER");
                        xml.WriteAttributeString("answer_id", strCorrectAnswerId);
                        xml.WriteEndElement();
                    }
                }

                xml.WriteEndElement();//Gradable
            }

            xml.WriteEndElement(); //end question class name
        }

        /// <summary>
        /// Write the current date/time
        /// </summary>
        /// <param name="xml"></param>
        private void WriteDates(XmlTextWriter xml)
        {
            //Dates
            xml.WriteStartElement("DATES");
            xml.WriteStartElement("CREATED");
            xml.WriteAttributeString("value", DateTime.UtcNow.ToString("u"));
            xml.WriteEndElement();

            xml.WriteStartElement("UPDATED");
            xml.WriteAttributeString("value", DateTime.UtcNow.ToString("u"));
            xml.WriteEndElement();
            xml.WriteEndElement();
        }

        /// <summary>
        /// Writes the automatic feedback for questions that can be graded
        /// </summary>
        /// <param name="xml"></param>
        private void WriteFeedback(XmlTextWriter xml)
        {
            xml.WriteStartElement("FEEDBACK_WHEN_CORRECT");
            xml.WriteString("Good work");
            xml.WriteEndElement();

            xml.WriteStartElement("FEEDBACK_WHEN_INCORRECT");
            xml.WriteString("That's not correct");
            xml.WriteEndElement();
        }

        /// <summary>
        /// Get the Blackboard class name for a question type
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        private string GetQuestionClass(Question question)
        {
            switch (question.Type)
            {
                case QuestionType.QT_MULTIPLE_ANSWER:
                    return "QUESTION_MULTIPLEANSWER";
                case QuestionType.QT_MULTIPLE_CHOICE:
                    return "QUESTION_MULTIPLECHOICE";
                case QuestionType.QT_TRUE_OR_FALSE:
                    return "QUESTION_TRUEFALSE";
                case QuestionType.QT_ESSAY:
                    return "QUESTION_ESSAY";
                case QuestionType.QT_FILL_IN_THE_BLANK:
                    return "QUESTION_FILLINBLANK";
                case QuestionType.QT_MATCHING:
                    return "QUESTION_MATCH";
                default:
                    //Type unknown
                    return "";
            }
        }
    }
}

/* Sample XML -- Note: This sample includes a matching statement

<?xml version="1.0" encoding="utf-8"?>
<POOL>
  <COURSEID value="IMPORT" />
  <TITLE value="test quiz" />
  <DESCRIPTION>
    <TEXT>Created by the CSI Blackboard Quiz Generator</TEXT>
  </DESCRIPTION>
  <DATES>
    <CREATED value="2011-11-17 20:26:37Z" />
    <UPDATED value="2011-11-17 20:26:37Z" />
  </DATES>
  <QUESTIONLIST>
    <QUESTION id="q1" class="QUESTION_MULTIPLEANSWER" />
    <QUESTION id="q2" class="QUESTION_MULTIPLECHOICE" />
    <QUESTION id="q3" class="QUESTION_TRUEFALSE" />
    <QUESTION id="q4" class="QUESTION_ESSAY" />
    <QUESTION id="q5" class="QUESTION_FILLINBLANK" />
    <QUESTION id="q6" class="QUESTION_MATCH" />
  </QUESTIONLIST>
  <QUESTION_MULTIPLEANSWER id="q1">
    <DATES>
      <CREATED value="2011-11-17 20:26:37Z" />
      <UPDATED value="2011-11-17 20:26:37Z" />
    </DATES>
    <BODY>
      <TEXT>Which of the following is a prime number?</TEXT>
      <FLAGS value="true">
        <ISHTML value="true" />
        <ISNEWLINELITERAL />
      </FLAGS>
    </BODY>
    <ANSWER id="q1_a1" position="1">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>2</TEXT>
    </ANSWER>
    <GRADABLE>
      <FEEDBACK_WHEN_CORRECT>Good work</FEEDBACK_WHEN_CORRECT>
      <FEEDBACK_WHEN_INCORRECT>That's not correct</FEEDBACK_WHEN_INCORRECT>
      <CORRECTANSWER answer_id="q1_a1" />
      <CORRECTANSWER answer_id="q1_a2" />
      <CORRECTANSWER answer_id="q1_a4" />
      <CORRECTANSWER answer_id="q1_a6" />
    </GRADABLE>
    <ANSWER id="q1_a2" position="2">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>3</TEXT>
    </ANSWER>
    <ANSWER id="q1_a3" position="3">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>4</TEXT>
    </ANSWER>
    <ANSWER id="q1_a4" position="4">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>5</TEXT>
    </ANSWER>
    <ANSWER id="q1_a5" position="5">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>6</TEXT>
    </ANSWER>
    <ANSWER id="q1_a6" position="6">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>7</TEXT>
    </ANSWER>
  </QUESTION_MULTIPLEANSWER>
  <QUESTION_MULTIPLECHOICE id="q2">
    <DATES>
      <CREATED value="2011-11-17 20:26:37Z" />
      <UPDATED value="2011-11-17 20:26:37Z" />
    </DATES>
    <BODY>
      <TEXT>Which of the following is a prime number?</TEXT>
      <FLAGS value="true">
        <ISHTML value="true" />
        <ISNEWLINELITERAL />
      </FLAGS>
    </BODY>
    <ANSWER id="q2_a1" position="1">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>4</TEXT>
    </ANSWER>
    <GRADABLE>
      <FEEDBACK_WHEN_CORRECT>Good work</FEEDBACK_WHEN_CORRECT>
      <FEEDBACK_WHEN_INCORRECT>That's not correct</FEEDBACK_WHEN_INCORRECT>
      <CORRECTANSWER answer_id="q2_a2" />
    </GRADABLE>
    <ANSWER id="q2_a2" position="2">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>5</TEXT>
    </ANSWER>
    <ANSWER id="q2_a3" position="3">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>6</TEXT>
    </ANSWER>
  </QUESTION_MULTIPLECHOICE>
  <QUESTION_TRUEFALSE id="q3">
    <DATES>
      <CREATED value="2011-11-17 20:26:37Z" />
      <UPDATED value="2011-11-17 20:26:37Z" />
    </DATES>
    <BODY>
      <TEXT>3 is a prime number.</TEXT>
      <FLAGS value="true">
        <ISHTML value="true" />
        <ISNEWLINELITERAL />
      </FLAGS>
    </BODY>
    <ANSWER id="q3_a1" position="1">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>True</TEXT>
    </ANSWER>
    <GRADABLE>
      <FEEDBACK_WHEN_CORRECT>Good work</FEEDBACK_WHEN_CORRECT>
      <FEEDBACK_WHEN_INCORRECT>That's not correct</FEEDBACK_WHEN_INCORRECT>
      <CORRECTANSWER answer_id="q3_a1" />
    </GRADABLE>
    <ANSWER id="q3_a2" position="2">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>False</TEXT>
    </ANSWER>
  </QUESTION_TRUEFALSE>
  <QUESTION_ESSAY id="q4">
    <DATES>
      <CREATED value="2011-11-17 20:26:37Z" />
      <UPDATED value="2011-11-17 20:26:37Z" />
    </DATES>
    <BODY>
      <TEXT>Tell me your life story.</TEXT>
      <FLAGS value="true">
        <ISHTML value="true" />
        <ISNEWLINELITERAL />
      </FLAGS>
    </BODY>
  </QUESTION_ESSAY>
  <QUESTION_FILLINBLANK id="q5">
    <DATES>
      <CREATED value="2011-11-17 20:26:37Z" />
      <UPDATED value="2011-11-17 20:26:37Z" />
    </DATES>
    <BODY>
      <TEXT>Two plus two equals _____.</TEXT>
      <FLAGS value="true">
        <ISHTML value="true" />
        <ISNEWLINELITERAL />
      </FLAGS>
    </BODY>
    <GRADABLE>
      <FEEDBACK_WHEN_CORRECT>Good work</FEEDBACK_WHEN_CORRECT>
      <FEEDBACK_WHEN_INCORRECT>That's not correct</FEEDBACK_WHEN_INCORRECT>
    </GRADABLE>
    <ANSWER id="q5_a1" position="1">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>four</TEXT>
    </ANSWER>
    <ANSWER id="q5_a2" position="2">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>4</TEXT>
    </ANSWER>
  </QUESTION_FILLINBLANK>
  <QUESTION_MATCH id="q6">
    <DATES>
      <CREATED value="2011-11-17 20:26:37Z" />
      <UPDATED value="2011-11-17 20:26:37Z" />
    </DATES>
    <BODY>
      <TEXT>Match the number with it's spelling. :)</TEXT>
      <FLAGS value="true">
        <ISHTML value="true" />
        <ISNEWLINELITERAL />
      </FLAGS>
    </BODY>
    <GRADABLE>
      <FEEDBACK_WHEN_CORRECT>Good work</FEEDBACK_WHEN_CORRECT>
      <FEEDBACK_WHEN_INCORRECT>That's not correct</FEEDBACK_WHEN_INCORRECT>
      <CORRECTANSWER answer_id="q6_a1" choice_id="q6_c1" />
      <CORRECTANSWER answer_id="q6_a2" choice_id="q6_c2" />
      <CORRECTANSWER answer_id="q6_a3" choice_id="q6_c3" />
      <CORRECTANSWER answer_id="q6_a4" choice_id="q6_c4" />
    </GRADABLE>
    <ANSWER id="q6_a3" placement="left" position="1">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>12</TEXT>
    </ANSWER>
    <ANSWER id="q6_a2" placement="left" position="2">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>1</TEXT>
    </ANSWER>
    <ANSWER id="q6_a1" placement="left" position="3">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>3</TEXT>
    </ANSWER>
    <ANSWER id="q6_a4" placement="left" position="4">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>4</TEXT>
    </ANSWER>
    <CHOICE id="q6_c3" placement="right" position="1">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>twelve</TEXT>
    </CHOICE>
    <CHOICE id="q6_c2" placement="right" position="2">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>one</TEXT>
    </CHOICE>
    <CHOICE id="q6_c1" placement="right" position="3">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>three</TEXT>
    </CHOICE>
    <CHOICE id="q6_c4" placement="right" position="4">
      <DATES>
        <CREATED value="2011-11-17 20:26:37Z" />
        <UPDATED value="2011-11-17 20:26:37Z" />
      </DATES>
      <TEXT>four</TEXT>
    </CHOICE>
  </QUESTION_MATCH>
</POOL>

*/