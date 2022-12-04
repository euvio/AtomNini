using AtomNini.Util;
using System;
using System.Collections;

namespace AtomNini
{
    internal class IniSectionCollection : ICollection, IEnumerable
    {
        #region Private variables

        private OrderedList list = new OrderedList();

        #endregion Private variables

        #region Public properties

        public IniSection this[int index]
        {
            get { return (IniSection)list[index]; }
        }

        public IniSection this[string configName]
        {
            get { return (IniSection)list[configName]; }
        }

        public int Count
        {
            get { return list.Count; }
        }

        public object SyncRoot
        {
            get { return list.SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return list.IsSynchronized; }
        }

        #endregion Public properties

        #region Public methods

        public void Add(IniSection section)
        {
            if (list.Contains(section))
            {
                throw new ArgumentException("IniSection already exists");
            }

            list.Add(section.Name, section);
        }

        public void Remove(string config)
        {
            list.Remove(config);
        }

        public void CopyTo(Array array, int index)
        {
            list.CopyTo(array, index);
        }

        public void CopyTo(IniSection[] array, int index)
        {
            ((ICollection)list).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion Public methods
    }
}