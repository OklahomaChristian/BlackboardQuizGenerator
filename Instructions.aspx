<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Instructions.aspx.cs" Inherits="Instructions" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!-- Style sheet -->
    <link rel='stylesheet' href='StyleSheets/instructions.css'/>

    <title>Blackboard Quiz Generator Instructions</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <table width="98%" border="0" cellspacing="0" cellpadding="0">
            <tr>
                <td>
                    <div>
                        <h3>Blackboard Quiz Generator Documentation</h3>
                        Below are the formatting rules to help you create questions that the quiz generator will understand.

                        <p><strong>Question Types:</strong></p>
                        <ul>
                          <li><a href="#mc">Multiple Choice</a></li>
                          <li><a href="#ma">Multiple Answer</a></li>
                          <li><a href="#tf">True/False</a></li>
                          <li><a href="#essay">Essay</a></li>
                          <li><a href="#blank">Fill in the blank</a></li>
                          
                        </ul>

                        <p><a name="mc"></a><strong>Multiple Choice</strong></p>
                        <ul>
                          <li>Question on a single line.</li>
                          <li>Answers immediately following the question.</li>
                          <li>Asterisk (*) in front of the correct choice.</li>
                          </ul>
                        <blockquote>
                          <p>Example:</p>
                          <p>1. Which of the following is a prime number?<br />
                             a) 4<br />
                            *b) 5<br />
                             c) 6 </p>
                        </blockquote>

                        <p><a name="ma"></a><strong>Multiple Answer</strong></p>
                        <ul>
                          <li>Exactly the same as multiple choice, only with multiple answers marked correct. </li>
                          <li>Question on a single line.</li>
                          <li>Answers immediately following the question.</li>
                          <li>Asterisk (*) in front of all correct choices.</li>
                        </ul>
                        <blockquote>
                          <p>Example:</p>
                          <p>1. Which of the following is a prime number?<br />
                            *a) 2 <br />
                            *b) 3 <br />
                             d) 4 <br />
                            *e) 5 <br />
                             f) 6 <br />
                            *g) 7 </p>
                        </blockquote>

                        <p><a name="tf"></a><strong>True/False</strong></p>
                        <ul>
                          <li>Question on a single line</li>
                          <li>The answer on the next line. Can be any of the following: T, t, True, TRUE, true, F, f, False, FALSE, false</li>
                          </ul>
                        <blockquote>
                          <p>Example:</p>
                          <p>1. 3 is a prime number. <br />
                            True</p>
                        </blockquote>

                        <p><a name="essay"></a><strong>Essay</strong></p>
                        <ul>
                          <li>This is simply a question with no answer given.
                            <p>Example:</p>
                            <p>1. Tell me your life story.</p>
                          </li>
                        </ul>

                        <p><a name="blank"></a><strong>Fill in the blank</strong></p>
                        <ul>
                          <li>Begin your question with the keyword: blank </li>
                          <li>Leave a blank somewhere in the question.</li>
                          <li>Give all of the possible correct answers. 
                            <p>Example:</p>
                            <p>blank 1. Two plus two equals _____.<br />
                              a. four<br />
                              b. 4
                            </p>
                          </li>
                        </ul>

                        <!-- Not supporting matching at this time
                        <p><a name="match"></a><strong>Matching</strong> </p>
                        <ul>
                          <li>Begin your question with the keyword: match</li>
                          <li>Put the matches separated by slashes (/).</li>
                          <li>You can have entries with no match, i.e. leave the other side blank. </li>
                          <li>The entries will be randomized.                         
                            <p>Example:</p>
                            <p>match 1. Match the number with it's spelling. :)<br />
                              a. 3 / three<br />
                              b. 1 / one<br />
                              c. 12 / twelve<br />
                              d. 4 /<br />
                              e. / fore <br /></p>
                          </li>
                        </ul>
                        -->
                    </div>
                </td>
            </tr>
    </table>		
    </div>
    </form>
</body>
</html>
