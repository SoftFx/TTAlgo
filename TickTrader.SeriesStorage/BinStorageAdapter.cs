using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    internal class BinStorageAdapter<TKey, TValue> : ICollectionStorage<TKey, TValue>
    {
        private IBinaryStorageCollection<TKey> _binStorage;
        private IValueSerializer<TValue> _serializer;

        public BinStorageAdapter(IBinaryStorageCollection<TKey> binStorage, IValueSerializer<TValue> serializer)
        {
            _binStorage = binStorage;
            _serializer = serializer;
        }

        public void Dispose()
        {
            _binStorage.Dispose();
        }

        public void Drop()  
        {
            _binStorage.Drop();
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Iterate(TKey from)
        {
            foreach (var item in _binStorage.Iterate(from))
            {
                var value = _serializer.Deserialize(new ArraySegment<byte>(item.Value));
                yield return new KeyValuePair<TKey, TValue>(item.Key, value);
            }
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed)
        {
            return _binStorage.IterateKeys(from, reversed);
        }

        public bool Read(TKey key, out TValue value)
        {
            byte[] bytes;

            if (_binStorage.Read(key, out bytes))
            {
                value = default(TValue);
                return false;
            }

            value = _serializer.Deserialize(new ArraySegment<byte>(bytes));
            return true;
        }

        public void Remove(TKey key)
        {
            _binStorage.Remove(key);
        }

        public void RemoveAll()
        {
            _binStorage.RemoveAll();
        }

        public void Write(TKey key, TValue value)
        {
            var binVal = _serializer.Serialize(value);
            // TO DO : ToArray() causes bad performance
            _binStorage.Write(key, binVal.ToArray());
        }

        public long GetSize()
        {
            return _binStorage.GetSize();
        }
    }
}
