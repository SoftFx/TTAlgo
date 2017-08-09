using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public class BinStorageAdapter<TKey, TValue> : ISliceCollection<TKey, TValue>
    {
        private IBinaryStorageCollection<TKey> _binStorage;
        private ISliceSerializer<TKey, TValue> _serializer;

        public BinStorageAdapter(IBinaryStorageCollection<TKey> binStorage, ISliceSerializer<TKey, TValue> serializer)
        {
            _binStorage = binStorage;
            _serializer = serializer;
        }

        public ISlice<TKey, TValue> CreateSlice(TKey from, TKey to, ArraySegment<TValue> sliceContent)
        {
            return _serializer.CreateSlice(from, to, sliceContent);
        }

        public void Dispose()
        {
            _binStorage.Dispose();
        }

        public void Drop()
        {
            _binStorage.Drop();
        }

        public IEnumerable<KeyValuePair<TKey, ISlice<TKey, TValue>>> Iterate(TKey from, bool reversed)
        {
            foreach (var item in _binStorage.Iterate(from, reversed))
            {
                var value = _serializer.Deserialize(new ArraySegment<byte>(item.Value));
                yield return new KeyValuePair<TKey, ISlice<TKey, TValue>>(item.Key, value);
            }
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed)
        {
            throw new NotImplementedException();
        }

        public ISlice<TKey, TValue> Read(TKey key)
        {
            byte[] bytes = _binStorage.Read(key);

            if (bytes == null)
                return null;

            return _serializer.Deserialize(new ArraySegment<byte>(bytes));
        }

        public void Remove(TKey key)
        {
            _binStorage.Remove(key);
        }

        public void RemoveAll()
        {
            _binStorage.RemoveAll();
        }

        public void Write(TKey key, ISlice<TKey, TValue> value)
        {
            var binVal = _serializer.Serialize(value);
            _binStorage.Write(key, binVal);
        }
    }
}
