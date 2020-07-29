using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Machinarium.Qnil
{
    public partial class VarDictionary<TKey, TValue> : IVarSet<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _snapshot = new Dictionary<TKey, TValue>();
        private readonly KeyCollection _keySetProxy;

        public VarDictionary()
        {
            Snapshot = new SnapshpotAccessor(this);
            _keySetProxy = new KeyCollection(_snapshot.Keys);
        }

        public IReadOnlyDictionary<TKey, TValue> Snapshot { get; }
        public int Count =>_snapshot.Count;
        public IVarSet<TKey> Keys => _keySetProxy;
        public IEnumerable<TValue> Values =>_snapshot.Values;

        public event DictionaryUpdateHandler<TKey, TValue> Updated;

        public TValue this[TKey key]
        {
            get => _snapshot[key];
            set
            {
                if (_snapshot.TryGetValue(key, out TValue oldItem))
                {
                    _snapshot[key] = value;
                    OnUpdate(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Replace, key, value, oldItem));
                }
                else
                    Add(key, value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            _snapshot.Add(key, value);
            OnUpdate(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Insert, key, value));
            _keySetProxy.FireKeyAdded(key);
        }

        public bool TryGetValue(TKey key, out TValue value) => _snapshot.TryGetValue(key, out value);

        public bool Remove(TKey key)
        {
            if (!_snapshot.TryGetValue(key, out TValue oldItem))
                return false;

            _snapshot.Remove(key);

            OnUpdate(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Remove, key, default(TValue), oldItem));
            _keySetProxy.FireKeyRemoved(key);
            return true;
        }

        public bool ContainsKey(TKey key) => _snapshot.ContainsKey(key);

        public void Dispose() { }

        public void Clear()
        {
            while (_snapshot.Count > 0)
                Remove(_snapshot.Keys.First());
        }

        private void OnUpdate(DictionaryUpdateArgs<TKey, TValue> args) => Updated?.Invoke(args);

        private sealed class SnapshpotAccessor : IReadOnlyDictionary<TKey, TValue>
        {
            private readonly VarDictionary<TKey, TValue> _parent;

            public SnapshpotAccessor(VarDictionary<TKey, TValue> parent)
            {
                _parent = parent;
            }

            public TValue this[TKey key] => _parent[key];

            public int Count => _parent.Count;

            public IEnumerable<TKey> Keys => _parent._snapshot.Keys;

            public IEnumerable<TValue> Values => _parent._snapshot.Values;

            public bool ContainsKey(TKey key) => _parent.ContainsKey(key);

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _parent._snapshot.GetEnumerator();

            public bool TryGetValue(TKey key, out TValue value) => _parent.TryGetValue(key, out value);

            IEnumerator IEnumerable.GetEnumerator() => _parent._snapshot.GetEnumerator();
        }
    }
}
