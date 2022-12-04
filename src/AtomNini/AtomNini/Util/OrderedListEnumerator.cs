using System;
using System.Collections;

namespace AtomNini.Util
{
    internal class OrderedListEnumerator : IDictionaryEnumerator
    {
        #region Private variables

        private int index = -1;
        private ArrayList list;

        #endregion Private variables

        #region Constructors

        /// <summary>
        /// Instantiates an ordered list enumerator with an ArrayList.
        /// </summary>
        internal OrderedListEnumerator(ArrayList arrayList)
        {
            list = arrayList;
        }

        #endregion Constructors

        #region Public properties

        object IEnumerator.Current
        {
            get
            {
                if (index < 0 || index >= list.Count)
                    throw new InvalidOperationException();

                return list[index];
            }
        }

        public DictionaryEntry Current
        {
            get
            {
                if (index < 0 || index >= list.Count)
                    throw new InvalidOperationException();

                return (DictionaryEntry)list[index];
            }
        }

        public DictionaryEntry Entry
        {
            get { return (DictionaryEntry)Current; }
        }

        public object Key
        {
            get { return Entry.Key; }
        }

        public object Value
        {
            get { return Entry.Value; }
        }

        #endregion Public properties

        #region Public methods

        public bool MoveNext()
        {
            index++;
            if (index >= list.Count)
                return false;

            return true;
        }

        public void Reset()
        {
            index = -1;
        }

        #endregion Public methods
    }
}