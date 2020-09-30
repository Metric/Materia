using System;
using System.Collections.Generic;
using System.Text;

namespace DelaunayTriangulator
{
    class Set<T> : IEnumerable<T>
    {
        SortedList<T, int> list;

        public Set()
        {
            list = new SortedList<T, int>();
        }

        public void Add(T k)
        {
            if (!list.ContainsKey(k))
                list.Add(k, 0);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public void DeepCopy(Set<T> other)
        {
            list.Clear();
            foreach(T k in other.list.Keys)
                Add(k);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return list.Keys.GetEnumerator();
        }

        public void Clear()
        {
            list.Clear();
        }


        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
