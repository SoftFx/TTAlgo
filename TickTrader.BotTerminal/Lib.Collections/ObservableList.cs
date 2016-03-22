using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    [Serializable]
    public class ObservableList<T> : IList<T>, IObservableList<T>, IReadonlyObservableList<T>, IReadOnlyList<T>
    {
        private List<T> innerList;

        public ObservableList()
        {
            innerList = new List<T>();
        }

        public ObservableList(IEnumerable<T> initialData)
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

                if (Changed != null)
                    Changed(this, new ListChangedEventArgs<T>(CollectionChangeActions.Replaced, value, oldItem, index));
            }
        }

        public int Count { get { return innerList.Count; } }

        public bool IsReadOnly { get { return false; } }

        public event EventHandler<ListChangedEventArgs<T>> Changed;

        public void Add(T item)
        {
            innerList.Add(item);
            int index = innerList.Count - 1;
            if (Changed != null)
                Changed(this, new ListChangedEventArgs<T>(CollectionChangeActions.Added, item, default(T), index));
        }

        public void Clear()
        {
            var oldItems = innerList;
            innerList = new List<T>();

            if (Changed != null)
            {
                for (int i = 0; i < oldItems.Count; i++)
                {
                    var item = oldItems[i];
                    Changed(this, new ListChangedEventArgs<T>(CollectionChangeActions.Removed, default(T), item, i));
                }
            }
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
            if (Changed != null)
                Changed(this, new ListChangedEventArgs<T>(CollectionChangeActions.Added, item, default(T), index));
        }

        public bool Remove(T item)
        {
            int index = innerList.IndexOf(item);

            if (index < 0)
                return false;

            T removedItem = innerList[index];

            if (Changed != null)
                Changed(this, new ListChangedEventArgs<T>(CollectionChangeActions.Removed, default(T), removedItem, index));

            return true;
        }

        public void RemoveAt(int index)
        {
            var removedItem = innerList[index];
            innerList.RemoveAt(index);
            if (Changed != null)
                Changed(this, new ListChangedEventArgs<T>(CollectionChangeActions.Removed, default(T), removedItem, index));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
    }
}
