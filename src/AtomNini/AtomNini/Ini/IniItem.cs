namespace AtomNini
{
    internal class IniItem
    {
        #region Private variables

        private IniType iniType = IniType.Empty;
        private string iniName = "";
        private string iniValue = "";
        private string iniComment = null;

        #endregion Private variables

        #region Public properties

        public IniType Type
        {
            get { return iniType; }
            set { iniType = value; }
        }

        public string Value
        {
            get { return iniValue; }
            set { iniValue = value; }
        }

        public string Name
        {
            get { return iniName; }
        }

        public string Comment
        {
            get { return iniComment; }
            set { iniComment = value; }
        }

        #endregion Public properties

        protected internal IniItem(string name, string value, IniType type, string comment)
        {
            iniName = name;
            iniValue = value;
            iniType = type;
            iniComment = comment;
        }
    }
}