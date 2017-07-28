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

        public void Dispose()
        {
            _binStorage.Dispose();
        }

        public void Drop()
        {
            throw new NotImplementedException();
        }

        public IStorageIterator<TKey, ISlice<TKey, TValue>> Iterate(TKey from, bool reversed)
        {
            var binIterator = _binStorage.Iterate(from, reversed);
            return new IteratorAdapter { BinIterator = binIterator, Serializer = _serializer };
        }

        public StorageResultCodes Read(TKey key, out ISlice<TKey, TValue> value)
        {
            ArraySegment<byte> bytes;
            var rCode = _binStorage.Read(key, out bytes);

            if (rCode != StorageResultCodes.Ok)
            {
                value = null;
                return rCode;
            }

            value = _serializer.Deserialize(bytes);
            return StorageResultCodes.Ok;
        }

        public StorageResultCodes Remove(TKey key)
        {
            return _binStorage.Remove(key);
        }

        public StorageResultCodes Write(TKey key, ISlice<TKey, TValue> slice)
        {
            var binVal = _serializer.Serialize(slice);
            return _binStorage.Write(key, binVal);
        }

        private class IteratorAdapter : IStorageIterator<TKey, ISlice<TKey, TValue>>
        {
            public ISliceSerializer<TKey, TValue> Serializer { get; set; }
            public IStorageIterator<TKey, ArraySegment<byte>> BinIterator { get; set; }

            public TKey Key => BinIterator.Key;
            public ISlice<TKey, TValue> Value => Serializer.Deserialize(BinIterator.Value);

            public void Dispose()
            {
                BinIterator.Dispose();
            }

            public StorageResultCodes Next()
            {
                return BinIterator.Next();
            }
        }
    }
}
