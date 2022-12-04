using System;

namespace AtomNini
{
    public class KeyNotExistException : Exception
    {
        public string FilePath { get; }
        public string Section { get; }

        public string Key { get; }

        public KeyNotExistException(string filePath, string section, string key)
        {
            FilePath = filePath;
            Section = section;
            Key = key;
        }

        public override string Message => $"{Key} does not exist under {Section} in {FilePath}.";
    }
}