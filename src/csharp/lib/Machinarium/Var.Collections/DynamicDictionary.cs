using System.Collections.Generic;
using System.Linq;

namespace Machinarium.Qnil
{
    public class VarDictionary<TKey, TValue> : IVarSet<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _snapshot = new Dictionary<TKey, TValue>();
        private readonly KeyCollection _keySetProxy;

        public VarDictionary()
        {
            _keySetProxy = new KeyCollection(_snapshot.Keys);
        }

        public IReadOnlyDictionary<TKey, TValue> Snapshot => _snapshot;

        public int Count => _snapshot.Count;

        public IVarSet<TKey> Keys => _keySetProxy;

        public IEnumerable<TValue> Values => _snapshot.Values;

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

            OnUpdate(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Remove, key, default, oldItem));
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

        private sealed class KeyCollection : IVarSet<TKey>
        {
            public KeyCollection(IEnumerable<TKey> snapshot)
            {
                Snapshot = snapshot;
            }

            public IEnumerable<TKey> Snapshot { get; }

            public event SetUpdateHandler<TKey> Updated;

            internal void FireKeyAdded(TKey key) => Updated?.Invoke(new SetUpdateArgs<TKey>(this, DLinqAction.Insert, key));

            internal void FireKeyRemoved(TKey key) => Updated?.Invoke(new SetUpdateArgs<TKey>(this, DLinqAction.Remove, default(TKey), key));

            public void Dispose()
            {
            }
        }
    }
}
