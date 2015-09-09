using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> innerDic = new Dictionary<TKey, TValue>();

        public event Action<KeyValuePair<TKey, TValue>> Added = delegate { };
        public event Action<TKey> Removed = delegate { };
        public event Action Cleared = delegate { };

        public void Add(TKey key, TValue value)
        {
            KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>(key, value);
            ((IDictionary<TKey, TValue>)innerDic).Add(pair);
            Added(pair);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)innerDic).Add(item);
            Added(item);
        }

        public bool Remove(TKey key)
        {
            if (innerDic.Remove(key))
            {
                Removed(key);
                return true;
            }
            return false;
        }

        public TValue this[TKey key]
        {
            get { return innerDic[key]; }
            set { innerDic[key] = value; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public void Clear()
        {
            innerDic.Clear();
        }

        public ReadonlyDictionaryObserver<TKey, TValue> AsReadonly()
        {
            return new ReadonlyDictionaryObserver<TKey, TValue>(this);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) { return  ((IDictionary<TKey, TValue>)innerDic).Contains(item); }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { ((IDictionary<TKey, TValue>)innerDic).CopyTo(array, arrayIndex); }
        public bool TryGetValue(TKey key, out TValue value) { return innerDic.TryGetValue(key, out value); }
        public ICollection<TValue> Values { get { return innerDic.Values; } }
        public ICollection<TKey> Keys { get { return innerDic.Keys; } }
        public int Count { get { return innerDic.Count; } }
        public bool IsReadOnly { get { return false; } }
        public bool ContainsKey(TKey key) { return innerDic.ContainsKey(key); }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return innerDic.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return innerDic.GetEnumerator(); }
    }

    public class ReadonlyDictionaryObserver<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private ObservableDictionary<TKey, TValue> observedDic;

        public ReadonlyDictionaryObserver(ObservableDictionary<TKey, TValue> observedDic)
        {
            this.observedDic = observedDic;
        }

        public event Action<KeyValuePair<TKey, TValue>> Added
        {
            add { observedDic.Added += value; }
            remove { observedDic.Added -= value; }
        }

        public event Action<TKey> Removed
        {
            add { observedDic.Removed += value; }
            remove { observedDic.Removed -= value; }
        }

        public event Action Cleared
        {
            add { observedDic.Cleared += value; }
            remove { observedDic.Cleared -= value; }
        }

        public bool ContainsKey(TKey key) { return observedDic.ContainsKey(key); }
        public IEnumerable<TKey> Keys { get { return observedDic.Keys; } }
        public bool TryGetValue(TKey key, out TValue value) { return observedDic.TryGetValue(key, out value); }
        public IEnumerable<TValue> Values { get { return observedDic.Values; } }
        public TValue this[TKey key] { get { return observedDic[key]; } }
        public int Count { get { return observedDic.Count; } }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return observedDic.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return observedDic.GetEnumerator(); }
    }
}
