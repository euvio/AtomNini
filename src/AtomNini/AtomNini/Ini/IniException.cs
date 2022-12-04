using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace AtomNini
{
    [Serializable]
    internal class IniException : SystemException /*, ISerializable */
    {
        #region Private variables

        private IniReader iniReader = null;
        private string message = "";

        #endregion Private variables

        #region Public properties

        public int LinePosition
        {
            get
            {
                return (iniReader == null) ? 0 : iniReader.LinePosition;
            }
        }

        public int LineNumber
        {
            get
            {
                return (iniReader == null) ? 0 : iniReader.LineNumber;
            }
        }

        public override string Message
        {
            get
            {
                if (iniReader == null)
                {
                    return base.Message;
                }

                return String.Format(CultureInfo.InvariantCulture, "{0} - Line: {1}, Position: {2}.",
                                        message, this.LineNumber, this.LinePosition);
            }
        }

        #endregion Public properties

        #region Constructors

        public IniException()
            : base()
        {
            this.message = "An error has occurred";
        }

        public IniException(string message, Exception exception)
            : base(message, exception)
        {
        }

        public IniException(string message)
            : base(message)
        {
            this.message = message;
        }

        internal IniException(IniReader reader, string message)
            : this(message)
        {
            iniReader = reader;
            this.message = message;
        }

        protected IniException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors

        #region Public methods

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info,
                                            StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (iniReader != null)
            {
                info.AddValue("lineNumber", iniReader.LineNumber);

                info.AddValue("linePosition", iniReader.LinePosition);
            }
        }

        #endregion Public methods
    }
}