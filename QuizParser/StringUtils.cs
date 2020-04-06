using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QuizParseLibrary
{
    /// <summary>
    /// Summary description for StringUtils
    /// </summary>
    public class StringUtils
    {
        /// <summary>
        /// Remove characters that may have been added by microsoft word/outlook
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string RemoveSpecialCharacters(string strInput)
        {
            string strResult = strInput;

            strResult = strResult.Replace("“", "\"");
            strResult = strResult.Replace("”", "\"");
            strResult = strResult.Replace("‘", "\'");
            strResult = strResult.Replace("’", "\'");

            //TODO: Find other special characters to remove

            //Special spaces

            return strResult;
        }

        /// <summary>
        /// Remove characters that are not acceptable for file names
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string GetFileSafeVersion(string strInput)
        {
            string strResult = strInput;

            strResult = strResult.Replace("\\", "");
            strResult = strResult.Replace("/", "");
            strResult = strResult.Replace("?", "");
            strResult = strResult.Replace("*", "");
            strResult = strResult.Replace(":", "");
            strResult = strResult.Replace("<", "");
            strResult = strResult.Replace(">", "");
            strResult = strResult.Replace("#", "");
            strResult = strResult.Replace("%", "");

            return strResult;
        }

        /// <summary>
        ///  Separate a string by double line breaks
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns> an array of strings</returns>
        public static string[] SeparateParagraphs(string strInput)
        {
            //Convert all those /r/n newline combinations to single newline characters
            string strNormalizedNewlines = Regex.Replace(strInput, @"(?:\r\n|[\r\n])", "\n");

            //Remove any blank lines
            string[] strLines = strNormalizedNewlines.Split('\n');

            for (int i = 0; i < strLines.Length; ++i)
            {
                strLines[i] = strLines[i].Trim();

                //Replace a blank line with a newline character
                if (strLines[i].Length == 0)
                    strLines[i] = "\n";
            }

            string strRemovedLines = String.Join("\n", strLines);

            //Split the string on double newline characters
            string[] strResults = Regex.Split(strRemovedLines, @"\n{2,}");

            return strResults;
        }
    }
}