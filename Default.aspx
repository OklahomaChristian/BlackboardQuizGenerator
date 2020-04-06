<%@ Page Title="" Language="C#" ValidateRequest="false" MasterPageFile="~/BlackboardTools.master" ViewStateMode="Enabled" ViewStateEncryptionMode="Never" EnableViewStateMac="false"  AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="QuizGenerator" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" Runat="Server">
    <!-- Reference the theme's stylesheet on the Google CDN -->
    <link href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.2/themes/smoothness/jquery-ui.css" type="text/css" rel="Stylesheet" />
 
    <!-- Reference jQuery and jQuery UI from the CDN. Remember that the order of these two elements is important -->
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js"></script>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.2/jquery-ui.min.js"></script>

	<!-- This one was good, but wasn't working with long single lines <script type="text/javascript" src='Javascript/jquery.autosize.js'></script> -->
    <script type="text/javascript" src='Javascript/popup.js'></script>
    <script type="text/javascript" src='Javascript/autogrow-textarea.js'></script>

    <script type="text/javascript">
        //Add a page load handler
        $(document).ready(onLoaded);

        //Page load handler - apply autosize to the controls that will use it
        function onLoaded() {
            //All text areas that contain the word question in their ID
            var textAreas = $('textarea[id*="question"]');
            textAreas.autogrow(); //.autosize();

            $('#popupContent').tabs();
        }
    </script>
	
	<script type="text/javascript">

	  var _gaq = _gaq || [];
	  _gaq.push(['_setAccount', 'UA-39122608-1']);
	  _gaq.push(['_trackPageview']);

	  (function() {
		var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
		ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
		var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
	  })();

	</script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="bodyContent" Runat="Server">
    <center>
        <b><asp:Label ID="lblError" ForeColor="Red" Text="" runat="server"></asp:Label></b>
        <br />
        <table style="background-color:#f6f6f6;padding:10px;">
            <tr>
                <td colspan="2" align="left">
                    <!-- Instructions Table -->
                    <table id="instructionsTable" border="0" width="100%" style="padding:0px;">
                        <tr>
                            <td colspan="2" style="text-align:center"><h2>BLACKBOARD TEST GENERATOR</h2><br /><br /></td>
                        </tr>
                        <tr>
                            <td align="left">
                                <h3>Instructions&nbsp</h3><br />
                            </td>
                             <td valign="middle" align="right">
                                <i>click the question mark for detailed instructions</i>&nbsp<a id="instructions" href="Default.aspx"><img src="images/helpIcon.png" alt="help" /></a>
                            </td>
                        </tr>
                        <tr>
                            <td align="left" colspan="2">
                                Type or paste your questions into the main text area and click the <b><i>Generate Test Questions</i></b> button.

                                <p><strong>Basic information:</strong></p>
					            <ul>
                                    <li>Questions start with a number followed by a period or parenthesis. </li>
                                    <li>Answers start with a letter followed by a period or parenthesis.</li>
					                <li style="width: 500px">If you enter questions/answers without numbers/letters, then questions must be tagged (MC for multiple choice, TF for true/false, MA for multiple answer, etc).</li>
                                    <li>Separate your questions with one blank line. </li>
					                <li style="width: 500px">Avoid leaving blank rows, except when you're going on to the next question or answer.</li>
					            </ul>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <br />
                    <hr />
                    <br />
                </td>
            </tr>
            <tr>
                <!-- Test name area -->
                <td align="left" >
                    <b>Test Name</b>
                    <br />
                    <asp:TextBox ID="txtQuizName" Width="200px" runat="server"></asp:TextBox>
                    <br />
                    <br />
                </td>
                <td align="right">
                    <b>Create sample question</b>
                    <br />
                    <asp:DropDownList ID="lstSampleQuestionTypes" Width="200px" runat="server"></asp:DropDownList>
                    <asp:Button ID="btnCreateSample" Width="40px" Text="OK" OnClick="OnCreateSample" runat="server" />
                </td>
            </tr>
            <tr>
                <!-- Quiz entry area -->
                <td align="center" colspan="2">
                    <asp:Panel ID="panelQuiz" Width="650px" Height="100%" ScrollBars="None" style="min-height:450px" runat="server">
                        <asp:TextBox ID="txtQuiz" TextMode="MultiLine" Width="98%" Height="450px" runat="server"></asp:TextBox>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td align="center" colspan="2">
                    <asp:Button ID="btnGenerate" Width="250px" Text="Generate Test Questions" runat="server" OnClick="OnGenerate" />
                    &nbsp;
                    <asp:Button ID="btnClear" Width="250px" Text="Clear Test Questions" runat="server" OnClick="OnClear" />
                </td>
            </tr>
        </table>

        <br />
        <!-- The download panel -->
        <asp:Panel ID="panelDownload" style="padding-top:0px" BackColor="White" BorderColor="Green" BorderWidth="1" width="650px" Height="120px" runat="server" Visible="false">
            <table width="70%" border="0">
                <tr>
                    <td align="center">
                        <asp:Button ID="btnDownloadTest" ToolTip="Download questions for the test area in Blackboard" Font-Bold="true" Text="Download Test Questions" Width="200px" Height="50px" OnClick="OnDownloadTest" runat="server" />
                    </td>
                    <td align="center">
                        <asp:Button ID="btnDownloadPool" ToolTip="Download questions for the pool area in Blackboard" Font-Bold="true" Text="Download Question Pool" Width="200px" Height="50px" OnClick="OnDownloadPool" runat="server" />
                    </td>
                </tr>
            </table>
            <br />
            <asp:Label id="lblDownload" Font-Italic="true" Font-Bold="true" runat="server"></asp:Label>
        </asp:Panel>
        <br />
    </center>

    <!-- The instructions popup -->
    <div id="popupContainer">
        <div id="popupHeader">
            <a id="popupContentClose"><b>x</b></a>
        </div>
        <div id="popupContent">
            <ul>
                <li><a href="#generalInstructions"><b>General Instructions</b></a></li>
                <li><a href="#questionHelp"><b>Question Formatting</b></a></li>
                <li><a href="#testUploadHelp"><b>Upload To Blackboard Test Area</b></a></li>
                <li><a href="#poolUploadHelp"><b>Upload to Blackboard Pool Area</b></a></li>
            </ul>
            <div id="generalInstructions">
                <iframe src="Instructions.htm" width="100%" height="600px" ></iframe>
            </div>
            <div id="questionHelp">
                <iframe src="QuestionTypes.htm" width="100%" height="600px" ></iframe>
            </div>
            <div id="testUploadHelp">
                <iframe src="UploadTestQuestions.htm" width="100%" height="600px"></iframe>
            </div>
            <div id="poolUploadHelp">
                <iframe src="UploadPool.htm" width="100%" height="600px"></iframe>
            </div>
        </div>
    </div>

    <div id="popupBackground">
        
    </div>
</asp:Content>