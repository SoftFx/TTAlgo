using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    [Serializable]
    public class DynamicList<T> : IList<T>, IDynamicList<T>, IReadOnlyList<T>
    {
        private List<T> innerList;

        public DynamicList()
        {
            innerList = new List<T>();
        }

        public DynamicList(IEnumerable<T> initialData)
        {
            innerList = new List<T>(initialData);
        }

        public T this[int index]
        {
            get { return innerList[index]; }
            set
            {
                T oldItem = innerList[index];
                innerList[index] = value;
                OnUpdate(new ListUpdateArgs<T>(this, DLinqUpdateType.Replace, index, value, oldItem));
            }
        }

        public int Count { get { return innerList.Count; } }
        public bool IsReadOnly { get { return false; } }

        public event ListUpdateHandler<T> Updated;

        public void Add(T item)
        {
            innerList.Add(item);
            int index = innerList.Count - 1;
            OnUpdate(new ListUpdateArgs<T>(this, DLinqUpdateType.Insert, index, item));
        }

        public void Clear()
        {
            innerList.Clear();
            OnUpdate(new ListUpdateArgs<T>(this, DLinqUpdateType.RemoveAll));
        }

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            innerList.Insert(index, item);
            OnUpdate(new ListUpdateArgs<T>(this, DLinqUpdateType.Insert, index, item));
        }

        public bool Remove(T item)
        {
            int index = innerList.IndexOf(item);

            if (index < 0)
                return false;

            T removedItem = innerList[index];
            OnUpdate(new ListUpdateArgs<T>(this, DLinqUpdateType.Remove, index, default(T), removedItem));

            return true;
        }

        public void RemoveAt(int index)
        {
            var removedItem = innerList[index];
            innerList.RemoveAt(index);
            OnUpdate(new ListUpdateArgs<T>(this, DLinqUpdateType.Remove, index, default(T), removedItem));
        }

        public void Dispose()
        {
            // do nothing
        }

        private void OnUpdate(ListUpdateArgs<T> args)
        {
            if (Updated != null)
                Updated(args);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
    }
}
