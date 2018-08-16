using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    internal class CollectionAdapter<TKey> : IBinaryStorageCollection<TKey>, IBinaryCollection
    {
        private IKeySerializer<TKey> _keySerializer;

        public CollectionAdapter(string name, IKeyValueBinaryStorage storage, IKeySerializer<TKey> keySerializer)
        {
            Name = name;
            Storage = storage;
            _keySerializer = keySerializer;
        }

        public string Name { get; }

        protected IKeyValueBinaryStorage Storage { get; set; }
        protected virtual void OnStartAccess() { }
        protected virtual void OnStopAccess() { }

        public virtual void Dispose()
        {
            Storage.Dispose();
        }

        public void Drop()
        {
            throw new NotSupportedException();
        }

        public long GetSize()
        {
            OnStartAccess();
            try
            {
                return Storage.GetSize();
            }
            finally
            {
                OnStopAccess();
            }
        }

        public IEnumerable<KeyValuePair<TKey, ArraySegment<byte>>> Iterate(bool reversed = false)
        {
            OnStartAccess();
            try
            {
                foreach (var pair in Storage.Iterate(reversed))
                {
                    var key = GetTypedKey(pair.Key);
                    var value = new ArraySegment<byte>(pair.Value);

                    yield return new KeyValuePair<TKey, ArraySegment<byte>>(key, value);
                }
            }
            finally
            {
                OnStopAccess();
            }
        }

        public IEnumerable<KeyValuePair<TKey, ArraySegment<byte>>> Iterate(TKey from, bool reversed = false)
        {
            OnStartAccess();
            try
            {
                foreach (var pair in Storage.Iterate(GetBinKey(from), reversed))
                {
                    var key = GetTypedKey(pair.Key);
                    var value = new ArraySegment<byte>(pair.Value);

                    yield return new KeyValuePair<TKey, ArraySegment<byte>>(key, value);
                }
            }
            finally
            {
                OnStopAccess();
            }
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed = false)
        {
            OnStartAccess();
            try
            {
                foreach (var k in Storage.IterateKeys(GetBinKey(from), reversed))
                    yield return GetTypedKey(k);
            }
            finally
            {
                OnStopAccess();
            }
        }

        public bool Read(TKey key, out ArraySegment<byte> value)
        {
            OnStartAccess();
            try
            {
                byte[] binValue;
                if (Storage.Read(GetBinKey(key), out binValue))
                {
                    value = new ArraySegment<byte>(binValue);
                    return true;
                }
                value = new ArraySegment<byte>();
                return false;
            }
            finally
            {
                OnStopAccess();
            }
        }

        public void Remove(TKey key)
        {
            OnStartAccess();
            try
            {
                Storage.Remove(GetBinKey(key));
            }
            finally
            {
                OnStopAccess();
            }
        }

        public void RemoveAll()
        {
            OnStartAccess();
            try
            {
                Storage.RemoveAll();
            }
            finally
            {
                OnStopAccess();
            }   
        }

        public void RemoveRange(TKey from, TKey to)
        {
            OnStartAccess();
            try
            {
                Storage.RemoveRange(GetBinKey(from), GetBinKey(to));
            }
            finally
            {
                OnStopAccess();
            }
        }

        public void Write(TKey key, ArraySegment<byte> value)
        {
            OnStartAccess();
            try
            {
                Storage.Write(GetBinKey(key), value.ToArray());
            }
            finally
            {
                OnStopAccess();
            }
        }

        private byte[] GetBinKey(TKey key)
        {
            var keyBuilder = new BinaryKeyWriter(_keySerializer.KeySize);
            _keySerializer.Serialize(key, keyBuilder);
            return keyBuilder.Buffer;
        }

        private TKey GetTypedKey(byte[] binKey)
        {
            var keyReader = new BinaryKeyReader(binKey);
            return _keySerializer.Deserialize(keyReader);
        }
    }
}
