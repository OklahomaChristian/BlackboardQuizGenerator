using System.Text;
using System.IO;

namespace QuizParseLibrary
{
    /// <summary>
    /// Hacked class to apply UTF8 encoding to the XML result
    /// </summary>
    public class StringWriterWithEncoding : StringWriter
    {
        private Encoding m_encoding;

        public override Encoding Encoding
        {
            get { return m_encoding; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="encoding"></param>
        public StringWriterWithEncoding(Encoding encoding)
            : base()
        {
            m_encoding = encoding;
        }
    }
}