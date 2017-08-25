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

        private byte[] _minKey;
        private byte[] _maxKey;

        public LevelDbCollection(string collectionName, ushort collectionId, LevelDB.DB database, IKeySerializer<TKey> keySerializer, bool isNew)
        {
            _collectionName = collectionName;
            _collectionId = collectionId;
            _database = database;
            _keySerializer = keySerializer;
            _isNew = isNew;

            _minKey = FillBinKey(byte.MinValue);
            _maxKey = FillBinKey(byte.MaxValue);
        }

        public string Name => _collectionName;
        public long ByteSize => GetSize();

        public IEnumerable<KeyValuePair<TKey, byte[]>> Iterate(TKey from)
        {
            byte[] refKey = GetBinKey(from);

            using (var dbIterator = _database.CreateIterator())
            {
                Seek(dbIterator, refKey);

                while (true)
                {
                    if (!dbIterator.Valid())
                        yield break; // end of db

                    TKey key;

                    if (!TryGetKey(dbIterator.Key(), out key))
                        yield break; // end of collection

                    yield return new KeyValuePair<TKey, byte[]>(key, dbIterator.Value());

                    dbIterator.Next();
                }
            }
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed)
        {
            byte[] refKey = GetBinKey(from);

            using (var dbIterator = _database.CreateIterator())
            {
                Seek(dbIterator, refKey);

                while (true)
                {
                    if (!dbIterator.Valid())
                        yield break; // end of db

                    TKey key;

                    if (!TryGetKey(dbIterator.Key(), out key))
                        yield break; // end of collection

                    yield return key;

                    dbIterator.Next();
                }
            }
        }

        public bool Read(TKey key, out byte[] value)
        {
            var binKey = GetBinKey(key);
            value = _database.Get(binKey);
            return value != null;
        }

        public void Write(TKey key, byte[] value)
        {
            EnsureNameHeaderWritten();
            var binKey = GetBinKey(key);
            _database.Put(binKey, value.ToArray());
        }

        public void Remove(TKey key)
        {
            var binKey = GetBinKey(key);
            _database.Delete(binKey);
        }

        public void RemoveAll()
        {
            throw new NotImplementedException();
        }

        public long GetSize()
        {
            return _database.GetApproximateSize(_minKey, _maxKey);
        }

        public void Drop()
        {
            RemoveAll();
            // TO DO : Drop header
            _isDisposed = true;
        }

        private void Seek(LevelDB.Iterator dbIterator, byte[] refKey)
        {
            dbIterator.Seek(refKey);

            if (dbIterator.Valid())
            {
                var iKey = dbIterator.Key();
                if (Enumerable.SequenceEqual(iKey, refKey))
                    return; // exact position

                // try step back
                dbIterator.Prev();

                if (dbIterator.Valid())
                {
                    TKey key;

                    if (TryGetKey(dbIterator.Key(), out key))
                        return; // prev key is from this collection 
                }

                dbIterator.Next();  // revert stepping back
            }
            else
            {
                // I have to use a hack :(
                // the only case seems to be end of base
                dbIterator.SeekToLast();
            }
        }

        private byte[] GetBinKey(TKey key)
        {
            var fullKeySize = 2 + _keySerializer.KeySize;
            var keyBuilder = new BinaryKeyWriter(fullKeySize);
            keyBuilder.WriteBe(_collectionId);
            _keySerializer.Serialize(key, keyBuilder);
            return keyBuilder.Buffer;
        }

        private byte[] FillBinKey(byte b)
        {
            var fullKeySize = 2 + _keySerializer.KeySize;
            var keyBuilder = new BinaryKeyWriter(fullKeySize);
            keyBuilder.WriteBe(_collectionId);
            for (int i = 0; i < _keySerializer.KeySize; i++)
                keyBuilder.Write(b);
            return keyBuilder.Buffer;
        }

        private bool TryGetKey(byte[] binKey, out TKey key)
        {
            var reader = new BinaryKeyReader(binKey);
            var collectionId = reader.ReadBeUshort();
            if (collectionId != _collectionId)
            {
                key = default(TKey);
                return false;
            }
            key = _keySerializer.Deserialize(reader);
            return true;
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
    }
}
