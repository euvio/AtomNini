using System;
using System.IO;
using System.Text;

namespace AtomNini
{
    #region IniWriteState enumeration

    internal enum IniWriteState : int
    {
        Start,

        BeforeFirstSection,

        Section,

        Closed
    };

    #endregion IniWriteState enumeration

    internal class IniWriter : IDisposable
    {
        #region Private variables

        private int indentation = 0;
        private bool useValueQuotes = false;
        private IniWriteState writeState = IniWriteState.Start;
        private char commentDelimiter = ';';
        private char assignDelimiter = '=';
        private TextWriter textWriter = null;
        private string eol = "\r\n";
        private StringBuilder indentationBuffer = new StringBuilder();
        private Stream baseStream = null;
        private bool disposed = false;

        #endregion Private variables

        #region Public properties

        public int Indentation
        {
            get { return indentation; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Negative values are illegal");

                indentation = value;
                indentationBuffer.Remove(0, indentationBuffer.Length);
                for (int i = 0; i < value; i++)
                    indentationBuffer.Append(' ');
            }
        }

        public bool UseValueQuotes
        {
            get { return useValueQuotes; }
            set { useValueQuotes = value; }
        }

        public IniWriteState WriteState
        {
            get { return writeState; }
        }

        public char CommentDelimiter
        {
            get { return commentDelimiter; }
            set { commentDelimiter = value; }
        }

        public char AssignDelimiter
        {
            get { return assignDelimiter; }
            set { assignDelimiter = value; }
        }

        public Stream BaseStream
        {
            get { return baseStream; }
        }

        #endregion Public properties

        #region Constructors

        public IniWriter(string filePath)
            : this(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
        }

        public IniWriter(TextWriter writer)
        {
            textWriter = writer;
            StreamWriter streamWriter = writer as StreamWriter;
            if (streamWriter != null)
            {
                baseStream = streamWriter.BaseStream;
            }
        }

        public IniWriter(Stream stream)
            : this(new StreamWriter(stream))
        {
        }

        #endregion Constructors

        #region Public methods

        public void Close()
        {
            textWriter.Close();
            writeState = IniWriteState.Closed;
        }

        public void Flush()
        {
            textWriter.Flush();
        }

        public override string ToString()
        {
            return textWriter.ToString();
        }

        public void WriteSection(string section)
        {
            ValidateState();
            writeState = IniWriteState.Section;
            WriteLine("[" + section + "]");
        }

        public void WriteSection(string section, string comment)
        {
            ValidateState();
            writeState = IniWriteState.Section;
            WriteLine("[" + section + "]" + Comment(comment));
        }

        public void WriteKey(string key, string value)
        {
            ValidateStateKey();
            WriteLine(key + " " + assignDelimiter + " " + GetKeyValue(value));
        }

        public void WriteKey(string key, string value, string comment)
        {
            ValidateStateKey();
            WriteLine(key + " " + assignDelimiter + " " + GetKeyValue(value) + Comment(comment));
        }

        public void WriteEmpty()
        {
            ValidateState();
            if (writeState == IniWriteState.Start)
            {
                writeState = IniWriteState.BeforeFirstSection;
            }
            WriteLine("");
        }

        public void WriteEmpty(string comment)
        {
            ValidateState();
            if (writeState == IniWriteState.Start)
            {
                writeState = IniWriteState.BeforeFirstSection;
            }
            if (comment == null)
            {
                WriteLine("");
            }
            else
            {
                WriteLine(commentDelimiter + " " + comment);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion Public methods

        #region Protected methods

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                textWriter.Close();
                baseStream.Close();
                disposed = true;

                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        #endregion Protected methods

        #region Private methods

        /// <summary>
        /// Destructor.
        /// </summary>
        ~IniWriter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Returns the value of a key.
        /// </summary>
        private string GetKeyValue(string text)
        {
            string result;

            if (useValueQuotes)
            {
                result = MassageValue('"' + text + '"');
            }
            else
            {
                result = MassageValue(text);
            }

            return result;
        }

        /// <summary>
        /// Validates whether a key can be written.
        /// </summary>
        private void ValidateStateKey()
        {
            ValidateState();

            switch (writeState)
            {
                case IniWriteState.BeforeFirstSection:
                case IniWriteState.Start:
                    throw new InvalidOperationException("The WriteState is not Section");
                case IniWriteState.Closed:
                    throw new InvalidOperationException("The writer is closed");
            }
        }

        /// <summary>
        /// Validates the state to determine if the item can be written.
        /// </summary>
        private void ValidateState()
        {
            if (writeState == IniWriteState.Closed)
            {
                throw new InvalidOperationException("The writer is closed");
            }
        }

        /// <summary>
        /// Returns a formatted comment.
        /// </summary>
        private string Comment(string text)
        {
            return (text == null) ? "" : (" " + commentDelimiter + " " + text);
        }

        /// <summary>
        /// Writes data to the writer.
        /// </summary>
        private void Write(string value)
        {
            textWriter.Write(indentationBuffer.ToString() + value);
        }

        /// <summary>
        /// Writes a full line to the writer.
        /// </summary>
        private void WriteLine(string value)
        {
            Write(value + eol);
        }

        /// <summary>
        /// Fixes the incoming value to prevent illegal characters from
        /// hurting the integrity of the INI file.
        /// </summary>
        private string MassageValue(string text)
        {
            return text.Replace("\n", "");
        }

        #endregion Private methods
    }
}