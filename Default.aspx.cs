using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Drawing;

using System.Text;
using System.IO;

using Ionic.Zip;

using QuizParseLibrary;

public partial class QuizGenerator : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        this.txtQuiz.CausesValidation = false;
        this.panelQuiz.BackColor = Color.White;

        if( false == IsPostBack )
            this.FillQuestionTypeDropDownList();
    }

    private void FillQuestionTypeDropDownList()
    {
        this.lstSampleQuestionTypes.Items.Clear();

        this.lstSampleQuestionTypes.Items.Add(new ListItem("Multiple Choice", ((int) QuestionType.QT_MULTIPLE_CHOICE).ToString() ) );
        this.lstSampleQuestionTypes.Items.Add(new ListItem("Multiple Answer", ((int) QuestionType.QT_MULTIPLE_ANSWER).ToString() ) );
        this.lstSampleQuestionTypes.Items.Add(new ListItem("True or False", ((int) QuestionType.QT_TRUE_OR_FALSE).ToString() ) );
        this.lstSampleQuestionTypes.Items.Add( new ListItem("Essay Question", ((int) QuestionType.QT_ESSAY).ToString() ) );
        this.lstSampleQuestionTypes.Items.Add( new ListItem("Fill in the Blank", ((int) QuestionType.QT_FILL_IN_THE_BLANK).ToString() ) );
		this.lstSampleQuestionTypes.Items.Add(new ListItem("Matching Question", ((int) QuestionType.QT_MATCHING).ToString() ) );

        /*
        QT_UNKNOWN = -1,
        QT_MULTIPLE_CHOICE,
        QT_MULTIPLE_ANSWER,
        QT_TRUE_OR_FALSE,
        QT_ESSAY,
        QT_FILL_IN_THE_BLANK,
        QT_MATCHING
         */
    }
 

    /// <summary>
    /// Get the text that makes up the quiz
    /// </summary>
    /// <returns></returns>
    private string GetQuizString()
    {
        string strQuiz = "";

        if (this.txtQuiz.Visible )
            strQuiz = this.txtQuiz.Text.Replace('\t', ' ');
        else
        {
            //Gather any dynamically generated textbox values from the response object
            int nQuestionIndex = 0;
            while( true )
            {
                string strQuestionID = "question_" + nQuestionIndex.ToString();
                NameValueCollection formObjects = this.Request.Form as NameValueCollection;

                string strValue = this.FindValueByPartialKey( formObjects, strQuestionID );

                if (strValue != null)
                {
                    strQuiz += strValue + "\n\n";
                    ++nQuestionIndex;
                }
                else
                    break;
            }
        }

        return strQuiz.Trim(); //Remove the extra whitespace
    }

    /// <summary>
    /// Find the first instance of a collection with a key that contains a partial key
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="strPartialKey"></param>
    /// <returns></returns>
    private string FindValueByPartialKey(NameValueCollection collection, string strPartialKey)
    {
        foreach (string strKey in collection.Keys)
        {
            if (strKey.Contains(strPartialKey))
                return collection[strKey];
        }

        return null;
    }

    /// <summary>
    /// add a textbox for each perceived question
    /// </summary>
    /// <param name="quiz"></param>
    private void AddQuestionTextBoxes( Quiz quiz )
    {
        int nQuestionIndex = 0;

        foreach (Question question in quiz.Questions)
        {
            //create a textbox for the question
            TextBox txtQuestion = new TextBox();
            txtQuestion.TextMode = TextBoxMode.MultiLine;
            txtQuestion.Width = new Unit("98%");
            txtQuestion.Text = question.RawText;
            txtQuestion.ID = "question_" + nQuestionIndex.ToString();
            txtQuestion.Rows = question.RawText.Split('\n').Length;

            nQuestionIndex++;

            if (question.IsValid())
            {
                txtQuestion.BackColor = Color.FromArgb(200, 255, 200);

                //Set the mouse over message to the question type
                string strType = question.GetQuestionTypeString();
                txtQuestion.ToolTip = "Valid question of type: " + strType;

            }
            else
            {
                txtQuestion.BackColor = Color.FromArgb(255, 200, 200);
                txtQuestion.ToolTip = question.ErrorMessage;
            }

            //Add the field to the panel
            this.panelQuiz.Controls.Add(txtQuestion);
        }
    }

    /// <summary>
    /// Write the contents of the quiz to a zip file in memory 
    /// </summary>
    /// <param name="quiz"></param>
    /// <returns>the memory stream containing the zip file</returns>
    private MemoryStream GetQuestionPoolZipStream(Quiz quiz)
    {
        UTF8Encoding encoding = new System.Text.UTF8Encoding();

        //The two required XML files - The first is a required scorm package file that never changes, the second is the quiz information XML
        string strManifestXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<manifest identifier=\"man00001\"><organization default=\"toc00001\"><tableofcontents identifier=\"toc00001\"/></organization><resources><resource baseurl=\"res00001\" file=\"res00001.dat\" identifier=\"res00001\" type=\"assessment/x-bb-pool\"/></resources></manifest>";
        string strQuizXML = quiz.GetBlackboardPoolXMLString();

        ZipFile zip = new ZipFile();
        zip.AddEntry("imsmanifest.xml", encoding.GetBytes(strManifestXML));
        zip.AddEntry("res00001.dat", encoding.GetBytes(strQuizXML));

        MemoryStream ms = new MemoryStream();

        zip.Save(ms);
        return ms;
    }

    /// <summary>
    /// Get the test question bytes
    /// </summary>
    /// <param name="quiz"></param>
    /// <returns></returns>
    private byte[] GetTestQuestionBytes(Quiz quiz)
    {
        UTF8Encoding encoding = new System.Text.UTF8Encoding();

        string strQuestions = quiz.GetBlackboardTestQuestionString();
        byte[] bytes = encoding.GetBytes( strQuestions );

        return bytes;
    }

    /// <summary>
    /// Clear the quiz
    /// </summary>
    private void ClearInput()
    {
        //Reset the view
        this.txtQuiz.Visible = true;
        this.txtQuiz.Text = "";
        this.Session["quiz"] = null;
        this.panelDownload.Visible = false;
        this.lblError.Text = "";
        this.Session["questionTextBoxes"] = null;
    }

    //////////////////////////////////////////
    //Event handlers

    /// <summary>
    /// Generate button handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnGenerate(object sender, EventArgs e)
    {
        QuizParser parser = new QuizParser();
        Quiz quiz = null;

        string strQuiz = this.GetQuizString();

        if (strQuiz.Length == 0) //Don't bother attempting to create an empty quiz
        {
            this.ClearInput();
            return;
        }
        else if ( parser.GenerateQuiz( strQuiz, out quiz) ) //Try to generate the quiz
        {
            this.lblError.ForeColor = Color.Green;
            this.lblError.Text = "Test generation successful <br/>";
            //this.lblError.Text += "Scroll to the bottom to Download your questions for Blackboard.";

            quiz.QuizName = this.txtQuizName.Text;

            this.Session["quiz"] = quiz;
            this.panelDownload.Visible = true;
            this.lblDownload.Text = "Test Generated with " + quiz.Questions.Count + " Questions";
        }
        else
        {
            //Display the error table
            this.lblError.ForeColor = Color.Red;
            this.lblError.Text = "Test generation failed<br/>Hold your mouse cursor over any red box for more information";
            this.panelDownload.Visible = false;
        }

        //Draw the separate quiz text fields
        this.txtQuiz.Visible = false;

        if( quiz != null )
            this.AddQuestionTextBoxes(quiz);
    }

    /// <summary>
    /// Clear button handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnClear(object sender, EventArgs e)
    {
        this.ClearInput();
    }

    /// <summary>
    /// Pool Download handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnDownloadPool(object sender, EventArgs e)
    {
        Quiz quiz = this.Session["quiz"] as Quiz;

        //Update the quiz name, in case it has changed
        quiz.QuizName = this.FilterQuizName(this.txtQuizName.Text);       

        //Get the name for the file - base it on the quiz name if one was entered, and if 
        string strFileName = StringUtils.GetFileSafeVersion(quiz.QuizName); //Note: IIS might actually do this automagically
        if (strFileName.Length == 0)
            strFileName = "blackboardQuiz";

        MemoryStream ms = this.GetQuestionPoolZipStream(quiz);

        //Write the zip file to the response object
        Response.Clear();
        Response.ContentType = "application/x-zip";
        Response.AddHeader("content-disposition", "attachment;filename=" + strFileName + ".zip");

        ms.WriteTo(Response.OutputStream);
        Response.Flush();

        ms.Close();
        ms.Dispose();
    }

    /// <summary>
    /// Test Download handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnDownloadTest(object sender, EventArgs e)
    {
        Quiz quiz = this.Session["quiz"] as Quiz;

        //Update the quiz name, in case it has changed
        quiz.QuizName = this.FilterQuizName(this.txtQuizName.Text);

        //Get the name for the file - base it on the quiz name if one was entered, and if 
        string strFileName = StringUtils.GetFileSafeVersion(quiz.QuizName); //Note: IIS might actually do this automagically
        if (strFileName.Length == 0)
            strFileName = "blackboardQuiz";
        
        //Write the zip file to the response object
        Response.Clear();
        Response.ContentType = "text/plain";
        Response.AddHeader("content-disposition", "attachment;filename=" + strFileName + ".txt");
        byte[] fileBytes = this.GetTestQuestionBytes(quiz);
        Response.Write('\uFEFF'); //Specify the Byte Order Mark (BOM) So that special characters are interpretted correctly
        Response.OutputStream.Write( fileBytes, 0, fileBytes.Length);
        Response.Flush();

        Response.End();
    }

    private string FilterQuizName( string strOriginal )
    {
        //Replace spaces with underscores
        var quizName = String.Join("_", strOriginal.Trim().Split(' ') );

        //Remove special characters
        for (var i = 0; i < quizName.Length; ++i)
        {
            if (!char.IsLetterOrDigit(quizName[i]))
            {
                quizName = RemoveStringCharacter(quizName, i);
                --i;
            }
        }

        return quizName;
    }

    public string RemoveStringCharacter(string input, int index)
    {
        if (input == null)
            throw new ArgumentNullException("input");
        
        string newString = string.Empty;

        for( var i = 0; i < input.Length; ++i )
        {
            if( i != index )
                newString += input[i];
        }

        return newString;
    }

    /// <summary>
    /// Create a sample question
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnCreateSample(object sender, EventArgs e)
    {
        string strToAdd = this.GetQuizString();

        this.ClearInput();

        if (strToAdd.Length > 0)
            strToAdd += "\n\n";

        QuestionType type = (QuestionType)( int.Parse( this.lstSampleQuestionTypes.SelectedValue ) );

        switch (type)
        {
            case QuestionType.QT_ESSAY:
                strToAdd +=
                    "1. Tell me your life story.";
                break;
            case QuestionType.QT_FILL_IN_THE_BLANK:
                strToAdd += 
                    "blank 1. Two plus two equals _____.\n" + 
                    "a. four\n" +
                    "b. 4";
                break;
            case QuestionType.QT_MULTIPLE_ANSWER:
                strToAdd += 
                    "1. Which of the following is a prime number?\n" +
                    "*a) 2\n" +
                    "*b) 3\n" +
                    "d) 4\n" +
                    "*e) 5\n" +
                    "f) 6\n" +
                    "*g) 7";
                break;
            case QuestionType.QT_MULTIPLE_CHOICE:
                strToAdd += 
                    "1. Which of the following is a prime number?\n" +
                    "a) 4\n" +
                    "*b) 5\n" + 
                    "c) 6";
                break;
            case QuestionType.QT_TRUE_OR_FALSE:
                strToAdd +=
                    "1. 3 is a prime number. \n" +
                    "True";
                break;
            case QuestionType.QT_MATCHING:
				strToAdd +=
					"match 6. Match the chapters with their topics. \n" +
					"a) Chapter 13/Exception Handling\n" +
					"b) Chapter 16/File Processing\n" +
					"c) Chapter 10/OOP & Classes\n" +
					"d) Chapter 15/Chars, C-Strings, & the String Class\n" +
					"e) Chapter 12/Not covered in this class";
				break;
			case QuestionType.QT_UNKNOWN:
			default:
                break;
        }

        this.txtQuiz.Text = strToAdd;
    }
}