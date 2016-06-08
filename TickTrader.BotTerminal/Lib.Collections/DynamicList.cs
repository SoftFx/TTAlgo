using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    public class DynamicList<T> : IDynamicListSource<T>
    {
        private ValuesCollection accessor;
        [DataMember(Name = "Elements")]
        private List<T> innerList;

        public DynamicList()
        {
            innerList = new List<T>();
            Init(new StreamingContext());
        }

        public DynamicList(IEnumerable<T> initialData)
        {
            innerList = new List<T>(initialData);
            Init(new StreamingContext());
        }

        [OnDeserialized]
        private void Init(StreamingContext context)
        {
            accessor = new ValuesCollection(this);
        }

        public int Count { get { return innerList.Count; } }
        public ValuesCollection Values { get { return accessor; } }

        public void Add(T item)
        {
            innerList.Add(item);
            int index = innerList.Count - 1;
            OnUpdate(new ListUpdateArgs<T>(this, DLinqAction.Insert, index, item));
        }

        public void Clear()
        {
            for (int i = Count - 1; i >= 0; i--)
                RemoveAt(i);
        }

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            innerList.Insert(index, item);
            OnUpdate(new ListUpdateArgs<T>(this, DLinqAction.Insert, index, item));
        }

        public bool Remove(T item)
        {
            int index = innerList.IndexOf(item);

            if (index < 0)
                return false;

            T removedItem = innerList[index];
            innerList.RemoveAt(index);
            OnUpdate(new ListUpdateArgs<T>(this, DLinqAction.Remove, index, default(T), removedItem));

            return true;
        }

        public void RemoveAt(int index)
        {
            var removedItem = innerList[index];
            innerList.RemoveAt(index);
            OnUpdate(new ListUpdateArgs<T>(this, DLinqAction.Remove, index, default(T), removedItem));
        }

        public T this[int index]
        {
            get { return innerList[index]; }
            set
            {
                T oldItem = innerList[index];
                innerList[index] = value;
                OnUpdate(new ListUpdateArgs<T>(this, DLinqAction.Replace, index, value, oldItem));
            }
        }

        public event ListUpdateHandler<T> Updated;

        public void Dispose()
        {
            // do nothing
        }

        private void OnUpdate(ListUpdateArgs<T> args)
        {
            if (Updated != null)
                Updated(args);
        }

        IReadOnlyList<T> IDynamicListSource<T>.Snapshot { get { return accessor; } }

        public class ValuesCollection : IList<T>, IReadOnlyList<T>
        {
            private DynamicList<T> parent;

            internal ValuesCollection(DynamicList<T> list)
            {
                this.parent = list;
            }

            public T this[int index]
            {
                get { return parent[index]; }
                set { parent[index] = value; }
            }

            public int Count { get { return parent.innerList.Count; } }
            public bool IsReadOnly { get { return false; } }

            public void Add(T item)
            {
                parent.Add(item);
            }

            public void Clear()
            {
                parent.Clear();
            }

            public bool Contains(T item)
            {
                return parent.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                parent.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return parent.innerList.GetEnumerator();
            }

            public int IndexOf(T item)
            {
                return parent.innerList.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                parent.Insert(index, item);
            }

            public bool Remove(T item)
            {
                return parent.Remove(item);
            }

            public void RemoveAt(int index)
            {
                parent.RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return parent.innerList.GetEnumerator();
            }
        }
    }
}
