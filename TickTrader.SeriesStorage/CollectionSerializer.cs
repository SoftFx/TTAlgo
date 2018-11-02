using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    internal class CollectionSerializer<TKey, TValue> : ICollectionStorage<TKey, TValue>
    {
        private IBinaryStorageCollection<TKey> _binStorage;
        private IValueSerializer<TValue> _serializer;

        public CollectionSerializer(IBinaryStorageCollection<TKey> binStorage, IValueSerializer<TValue> serializer)
        {
            _binStorage = binStorage;
            _serializer = serializer;
        }

        public ITransaction StartTransaction()
        {
            return _binStorage.StartTransaction();
        }

        public void Dispose()
        {
            _binStorage.Dispose();
        }

        public void Drop()  
        {
            _binStorage.Drop();
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Iterate(bool reversed, ITransaction transaction)
        {
            foreach (var item in _binStorage.Iterate(reversed))
            {
                var value = _serializer.Deserialize(item.Value);
                yield return new KeyValuePair<TKey, TValue>(item.Key, value);
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Iterate(TKey from, bool reversed, ITransaction transaction)
        {
            foreach (var item in _binStorage.Iterate(from, reversed))
            {
                var value = _serializer.Deserialize(item.Value);
                yield return new KeyValuePair<TKey, TValue>(item.Key, value);
            }
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed, ITransaction transaction = null)
        {
            return _binStorage.IterateKeys(from, reversed);
        }

        public bool Read(TKey key, out TValue value, ITransaction transaction = null)
        {
            ArraySegment<byte> bytes;

            if (_binStorage.Read(key, out bytes))
            {
                value = default(TValue);
                return false;
            }

            value = _serializer.Deserialize(bytes);
            return true;
        }

        public void Remove(TKey key, ITransaction transaction = null)
        {
            _binStorage.Remove(key);
        }

        public void RemoveRange(TKey from, TKey to, ITransaction transaction = null)
        {
            _binStorage.RemoveRange(from, to);
        }

        public void RemoveAll(ITransaction transaction = null)
        {
            _binStorage.RemoveAll();
        }

        public void Write(TKey key, TValue value, ITransaction transaction = null)
        {
            var binVal = _serializer.Serialize(value);
            _binStorage.Write(key, binVal);
        }

        public long GetSize()
        {
            return _binStorage.GetSize();
        }
    }
}
