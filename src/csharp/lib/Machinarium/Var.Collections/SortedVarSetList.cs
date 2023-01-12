using System.Collections;
using System.Collections.Generic;

namespace Machinarium.Qnil
{
    public class SortedVarSetList<T> : IVarList<T>
    {
        private readonly List<T> _innerList;
        private readonly IComparer<T> _comparer;
        private readonly ValuesCollection _accessor;


        public T this[int index] => _innerList[index];

        public int Count => _innerList.Count;

        public ValuesCollection Values => _accessor;

        IReadOnlyList<T> IVarList<T>.Snapshot => _accessor;


        public event ListUpdateHandler<T> Updated;


        public SortedVarSetList() : this(Comparer<T>.Default) { }

        public SortedVarSetList(IComparer<T> comparer)
        {
            _innerList = new List<T>();
            _comparer = comparer;
            _accessor = new ValuesCollection(this);
        }


        public void Dispose() { } // do nothing

        public int Add(T item)
        {
            var index = _innerList.BinarySearch(item, _comparer);
            if (index < 0)
            {
                index = ~index;
                _innerList.Insert(index, item);
                OnUpdate(new ListUpdateArgs<T>(this, DLinqAction.Insert, index, item));
            }
            else
            {
                _innerList[index] = item;
                OnUpdate(new ListUpdateArgs<T>(this, DLinqAction.Replace, index, item));
            }
            return index;
        }

        public int Remove(T item)
        {
            var index = _innerList.BinarySearch(item, _comparer);
            if (index >= 0)
            {
                RemoveAt(index);
                return index;
            }
            return -1;
        }

        public void RemoveAt(int index)
        {
            var removedItem = _innerList[index];
            _innerList.RemoveAt(index);
            OnUpdate(new ListUpdateArgs<T>(this, DLinqAction.Remove, index, oldItem: removedItem));
        }

        public void Clear()
        {
            for (int i = Count - 1; i >= 0; i--)
                RemoveAt(i);
        }

        public bool Contains(T item) => IndexOf(item) < 0;

        public int IndexOf(T item)
        {
            var index = _innerList.BinarySearch(item, _comparer);
            return index < 0 ? -1 : index;
        }

        public void CopyTo(T[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);


        private void OnUpdate(ListUpdateArgs<T> args) => Updated?.Invoke(args);


        public class ValuesCollection : ICollection<T>, IReadOnlyList<T>
        {
            private readonly SortedVarSetList<T> _parent;


            internal ValuesCollection(SortedVarSetList<T> list)
            {
                _parent = list;
            }

            public T this[int index] { get => _parent[index]; }

            public int Count => _parent._innerList.Count;
            public bool IsReadOnly => false;

            public void Add(T item) => _parent.Add(item);

            public void Clear() => _parent.Clear();

            public bool Contains(T item) => _parent.Contains(item);

            public void CopyTo(T[] array, int arrayIndex) => _parent.CopyTo(array, arrayIndex);

            public int IndexOf(T item)
            {
                return _parent.IndexOf(item);
            }

            public bool Remove(T item) => _parent.Remove(item) >= 0;

            public void RemoveAt(int index) => _parent.RemoveAt(index);

            public IEnumerator<T> GetEnumerator() => _parent._innerList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _parent._innerList.GetEnumerator();
        }
    }
}
