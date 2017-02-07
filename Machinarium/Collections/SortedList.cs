using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Collections
{
    public class SortedList<T> : IReadOnlyList<T>
    {
        private List<T> innerList = new List<T>();
        private IComparer<T> comparer;

        public int Count { get { return innerList.Count; } }
        public T this[int index] { get { return innerList[index]; } }

        public SortedList()
        {
            this.comparer = Comparer<T>.Default;
        }

        public SortedList(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        public int Add(T item)
        {
            int index = innerList.BinarySearch(item, comparer);
            int insertIndex = index < 0 ? ~index : index + 1;
            innerList.Insert(insertIndex, item);
            return insertIndex;
        }

        public int Remove(T item)
        {
            int index = innerList.BinarySearch(item, comparer);
            if (index >= 0)
                innerList.RemoveAt(index);
            return index;
        }

        public void RemoveAt(int index)
        {
            innerList.RemoveAt(index);
        }

        public int IndexOf(T item)
        {
            return innerList.BinarySearch(item, comparer);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
    }
}
