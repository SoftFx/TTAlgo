using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class ObservableSrotedList<TKey, TValue> : IReadOnlyList<TValue>, INotifyCollectionChanged
    {
        private SortedList<TKey, TValue> innerList = new SortedList<TKey, TValue>();

        public ObservableSrotedList()
        {
        }

        public void Add(TKey key, TValue val)
        {
            innerList.Add(key, val);
            int index = innerList.IndexOfKey(key);
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, val, index));
        }

        public bool Remove(TKey key)
        {
            TValue removedItem;
            return Remove(key, out removedItem);
        }

        public bool Remove(TKey key, out TValue removedItem)
        {
            int index = innerList.IndexOfKey(key);
            if (index < 0)
            {
                removedItem = default(TValue);
                return false;
            }
            removedItem = innerList.Values[index];
            innerList.RemoveAt(index);
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
            return true;
        }

        public void Upsert(TKey key, TValue val)
        {
            int index = innerList.IndexOfKey(key);

            if (index < 0)
                Add(key, val);
            else
            {
                innerList[key] = val;
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, val, index));
            }
        }

        public TValue GetOrDefault(TKey key)
        {
            TValue result;
            innerList.TryGetValue(key, out result);
            return result;
        }

        public bool ContainsKey(TKey key)
        {
            return innerList.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue val)
        {
            return innerList.TryGetValue(key, out val);
        }

        public void Clear()
        {
            innerList.Clear();
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };

        public TValue this[int index]
        {
            get { return innerList.Values[index]; }
        }

        public int Count { get{return innerList.Count;}}
        public IEnumerator<TValue> GetEnumerator() { return innerList.Values.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return innerList.Values.GetEnumerator(); }
    }
}
