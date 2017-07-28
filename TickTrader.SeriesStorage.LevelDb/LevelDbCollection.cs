using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.LevelDb
{
    internal class LevelDbCollection<TKey> : IBinaryStorageCollection<TKey>, IBinaryCollection
    {
        private ushort _collectionId;
        private string _collectionName;
        private LevelDB.DB _database;
        private IKeySerializer<TKey> _keySerializer;
        private bool _isNew;
        private bool _isDisposed;

        public LevelDbCollection(string collectionName, ushort collectionId, LevelDB.DB database, IKeySerializer<TKey> keySerializer, bool isNew)
        {
            _collectionName = collectionName;
            _collectionId = collectionId;
            _database = database;
            _keySerializer = keySerializer;
            _isNew = isNew;
        }

        public string Name => _collectionName;
        public long ByteSize => throw new NotImplementedException();

        public IStorageIterator<TKey, ArraySegment<byte>> Iterate(TKey from, bool reversed)
        {
            try
            {
                var dbIterator = _database.CreateIterator();
                var binFrom = GetBinKey(from);
                dbIterator.Seek(binFrom);
                return new Iterator(dbIterator, reversed, GetKey);
            }
            catch (LevelDB.LevelDBException)
            {
                return new ErrorIterator { ErrorCode = StorageResultCodes.Error };
            }
        }

        public StorageResultCodes Read(TKey key, out ArraySegment<byte> value)
        {
            try
            {
                var binKey = GetBinKey(key);
                var bytes = _database.Get(binKey);
                if (bytes == null)
                {
                    value = new ArraySegment<byte>();
                    return StorageResultCodes.ValueIsMissing;
                }

                value = new ArraySegment<byte>(bytes, 0, bytes.Length);
                return StorageResultCodes.Ok;
            }
            catch (LevelDB.LevelDBException)
            {
                value = new ArraySegment<byte>();
                return StorageResultCodes.Error;
            }
        }

        public StorageResultCodes Write(TKey key, ArraySegment<byte> value)
        {
            try
            {
                EnsureNameHeaderWritten();

                var binKey = GetBinKey(key);
                _database.Put(binKey, value.ToArray());
                return StorageResultCodes.Ok;
            }
            catch (LevelDB.LevelDBException)
            {
                return StorageResultCodes.Error;
            }
        }

        public StorageResultCodes Remove(TKey key)
        {
            try
            {
                var binKey = GetBinKey(key);
                _database.Delete(binKey);
                return StorageResultCodes.Ok;
            }
            catch (LevelDB.LevelDBException)
            {
                return StorageResultCodes.Error;
            }
        }

        private byte[] GetBinKey(TKey key)
        {
            var keyBuilder = new BinaryKeyStream();
            _keySerializer.Serialize(key, keyBuilder);
            return keyBuilder.ToArray();
        }

        private TKey GetKey(byte[] binKey)
        {
            var reader = new BinaryKeyReader(binKey);
            return _keySerializer.Deserialize(reader);
        }

        private void EnsureNameHeaderWritten()
        {
            if (_isNew)
            {
                var nameHeader = new Header(_collectionId, HeaderTypes.TableName, Encoding.UTF8.GetBytes(_collectionName));
                WriteHeader(nameHeader);
                _isNew = false;
            }
        }

        private void WriteHeader(Header h)
        {
            _database.Put(h.GetBinaryKey(), h.Content);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Collection");
        }

        public void Dispose()
        {
            _isDisposed = true;
        }

        public void Drop()
        {
            _isDisposed = true;
        }

        private class Iterator : IStorageIterator<TKey, ArraySegment<byte>>
        {
            private LevelDB.Iterator _dbIterator;
            private bool _reversed;
            private Func<byte[], TKey> _keyDeserializeFunc;

            public Iterator(LevelDB.Iterator dbIterator, bool reversed, Func<byte[], TKey> keyDeserializeFunc)
            {
                _dbIterator = dbIterator;
                _reversed = reversed;
                _keyDeserializeFunc = keyDeserializeFunc;
            }

            public TKey Key => _keyDeserializeFunc(_dbIterator.Key());
            public ArraySegment<byte> Value => new ArraySegment<byte>(_dbIterator.Value());

            public void Dispose()
            {
                _dbIterator.Dispose();
            }

            public StorageResultCodes Next()
            {
                try
                {
                    if (!_reversed)
                        _dbIterator.Next();
                    else
                        _dbIterator.Prev();
                    return StorageResultCodes.Ok;
                }
                catch (LevelDB.LevelDBException)
                {
                    return StorageResultCodes.Error;
                }
            }
        }

        private class ErrorIterator : IStorageIterator<TKey, ArraySegment<byte>>
        {
            public StorageResultCodes ErrorCode { get; set; }
            public TKey Key => default(TKey);
            public ArraySegment<byte> Value => default(ArraySegment<byte>);

            public void Dispose()
            {
            }

            public StorageResultCodes Next()
            {
                return ErrorCode;
            }
        }
    }
}
