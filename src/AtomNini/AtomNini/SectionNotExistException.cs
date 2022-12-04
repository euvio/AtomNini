using System;

namespace AtomNini
{
    public class SectionNotExistException : Exception
    {
        public string FilePath { get; }
        public string Section { get; }

        public SectionNotExistException(string filePath, string section)
        {
            FilePath = filePath;
            Section = section;
        }

        public override string Message => $"{Section} does not exist in {FilePath}.";
    }
}