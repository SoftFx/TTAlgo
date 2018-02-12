using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    public partial class DynamicDictionary<TKey, TValue> : IDynamicDictionarySource<TKey, TValue>
    {
        private Dictionary<TKey, TValue> snapshot = new Dictionary<TKey, TValue>();
        private KeyCollection _keySetProxy;

        public DynamicDictionary()
        {
            Snapshot = new SnapshpotAccessor(this);
            _keySetProxy = new KeyCollection(snapshot.Keys);
        }

        public IReadOnlyDictionary<TKey, TValue> Snapshot { get; private set; }
        public int Count { get { return snapshot.Count; } }
        public IDynamicSetSource<TKey> Keys => _keySetProxy;
        public IEnumerable<TValue> Values { get { return snapshot.Values; } }

        public event DictionaryUpdateHandler<TKey, TValue> Updated;

        public TValue this[TKey key]
        {
            get { return snapshot[key]; }
            set
            {
                TValue oldItem;
                if (snapshot.TryGetValue(key, out oldItem))
                {
                    snapshot[key] = value;
                    OnUpdate(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Replace, key, value, oldItem));
                }
                else
                    Add(key, value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            snapshot.Add(key, value);
            OnUpdate(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Insert, key, value));
            _keySetProxy.FireKeyAdded(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return snapshot.TryGetValue(key, out value);
        }

        public bool Remove(TKey key)
        {
            TValue oldItem;
            if (!snapshot.TryGetValue(key, out oldItem))
                return false;

            snapshot.Remove(key);

            OnUpdate(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Remove, key, default(TValue), oldItem));
            _keySetProxy.FireKeyRemoved(key);
            return true;
        }

        public bool ContainsKey(TKey key)
        {
            return snapshot.ContainsKey(key);
        }

        public void Dispose()
        {
        }

        public void Clear()
        {
            while (snapshot.Count > 0)
                Remove(snapshot.Keys.First());
        }

        private void OnUpdate(DictionaryUpdateArgs<TKey, TValue> args)
        {
            Updated?.Invoke(args);
        }

        private class SnapshpotAccessor : IReadOnlyDictionary<TKey, TValue>
        {
            private DynamicDictionary<TKey, TValue> parent;

            public SnapshpotAccessor(DynamicDictionary<TKey, TValue> parent)
            {
                this.parent = parent;
            }

            public TValue this[TKey key] { get { return parent[key]; } }
            public int Count { get { return parent.Count; } }
            public IEnumerable<TKey> Keys { get { return parent.snapshot.Keys; } }
            public IEnumerable<TValue> Values { get { return parent.snapshot.Values; } }

            public bool ContainsKey(TKey key)
            {
                return parent.ContainsKey(key);
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return parent.snapshot.GetEnumerator();
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return parent.TryGetValue(key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return parent.snapshot.GetEnumerator();
            }
        }
    }
}
