using System;
using System.Collections;
using System.IO;
using System.Text;

namespace AtomNini
{
    #region IniFileType enumeration

    public enum IniFileType
    {
        Standard,

        PythonStyle,

        SambaStyle,

        MysqlStyle,

        WindowsStyle
    }

    #endregion IniFileType enumeration

    internal class IniDocument
    {
        #region Private variables

        private IniSectionCollection sections = new IniSectionCollection();
        private ArrayList initialComment = new ArrayList();
        private IniFileType fileType = IniFileType.Standard;

        #endregion Private variables

        #region Public properties

        public IniFileType FileType
        {
            get { return fileType; }
            set { fileType = value; }
        }

        #endregion Public properties

        #region Constructors

        public IniDocument(TextReader reader, IniFileType type)
        {
            fileType = type;
            Load(reader);
        }

        #endregion Constructors

        #region Public methods

        public void Load(string filePath, Encoding encoding)
        {
            Load(new StreamReader(filePath, encoding));
        }

        public void Load(TextReader reader)
        {
            Load(GetIniReader(reader, fileType));
        }

        public void Load(Stream stream)
        {
            Load(new StreamReader(stream));
        }

        public void Load(IniReader reader)
        {
            LoadReader(reader);
        }

        public IniSectionCollection Sections
        {
            get { return sections; }
        }

        public void Save(TextWriter textWriter)
        {
            IniWriter writer = GetIniWriter(textWriter, fileType);
            IniItem item = null;
            IniSection section = null;

            foreach (string comment in initialComment)
            {
                writer.WriteEmpty(comment);
            }

            for (int j = 0; j < sections.Count; j++)
            {
                section = sections[j];
                writer.WriteSection(section.Name, section.Comment);
                for (int i = 0; i < section.ItemCount; i++)
                {
                    item = section.GetItem(i);
                    switch (item.Type)
                    {
                        case IniType.Key:
                            writer.WriteKey(item.Name, item.Value, item.Comment);
                            break;

                        case IniType.Empty:
                            writer.WriteEmpty(item.Comment);
                            break;
                    }
                }
            }

            writer.Close();
        }

        public void Save(string filePath, Encoding encoding)
        {
            StreamWriter writer = new StreamWriter(filePath, false, encoding);
            Save(writer);
            writer.Close();
        }

        public void Save(Stream stream)
        {
            Save(new StreamWriter(stream));
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// Loads the file not saving comments.
        /// </summary>
        private void LoadReader(IniReader reader)
        {
            reader.IgnoreComments = false;
            bool sectionFound = false;
            IniSection section = null;

            try
            {
                while (reader.Read())
                {
                    switch (reader.Type)
                    {
                        case IniType.Empty:
                            if (!sectionFound)
                            {
                                initialComment.Add(reader.Comment);
                            }
                            else
                            {
                                section.Set(reader.Comment);
                            }

                            break;

                        case IniType.Section:
                            sectionFound = true;
                            // If section already exists then overwrite it
                            if (sections[reader.Name] != null)
                            {
                                sections.Remove(reader.Name);
                            }
                            section = new IniSection(reader.Name, reader.Comment);
                            sections.Add(section);

                            break;

                        case IniType.Key:
                            if (section.GetValue(reader.Name) == null)
                            {
                                section.Set(reader.Name, reader.Value, reader.Comment);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // Always close the file
                reader.Close();
            }
        }

        /// <summary>
        /// Returns a proper INI reader depending upon the type parameter.
        /// </summary>
        private IniReader GetIniReader(TextReader reader, IniFileType type)
        {
            IniReader result = new IniReader(reader);

            switch (type)
            {
                case IniFileType.Standard:
                    // do nothing
                    break;

                case IniFileType.PythonStyle:
                    result.AcceptCommentAfterKey = false;
                    result.SetCommentDelimiters(new char[] { ';', '#' });
                    result.SetAssignDelimiters(new char[] { ':' });
                    break;

                case IniFileType.SambaStyle:
                    result.AcceptCommentAfterKey = false;
                    result.SetCommentDelimiters(new char[] { ';', '#' });
                    result.LineContinuation = true;
                    break;

                case IniFileType.MysqlStyle:
                    result.AcceptCommentAfterKey = false;
                    result.AcceptNoAssignmentOperator = true;
                    result.SetCommentDelimiters(new char[] { '#' });
                    result.SetAssignDelimiters(new char[] { ':', '=' });
                    break;

                case IniFileType.WindowsStyle:
                    result.ConsumeAllKeyText = true;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Returns a proper IniWriter depending upon the type parameter.
        /// </summary>
        private IniWriter GetIniWriter(TextWriter reader, IniFileType type)
        {
            IniWriter result = new IniWriter(reader);

            switch (type)
            {
                case IniFileType.Standard:
                case IniFileType.WindowsStyle:
                    // do nothing
                    break;

                case IniFileType.PythonStyle:
                    result.AssignDelimiter = ':';
                    result.CommentDelimiter = '#';
                    break;

                case IniFileType.SambaStyle:
                case IniFileType.MysqlStyle:
                    result.AssignDelimiter = '=';
                    result.CommentDelimiter = '#';
                    break;
            }

            return result;
        }

        #endregion Private methods
    }
}