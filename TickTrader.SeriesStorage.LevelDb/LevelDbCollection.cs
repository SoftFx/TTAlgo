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
        private LevelDbStorage _storage;
        private IKeySerializer<TKey> _keySerializer;
        private bool _isNew;
        private bool _isDisposed;

        private byte[] _minKey;
        private byte[] _maxKey;

        public LevelDbCollection(string collectionName, ushort collectionId, LevelDbStorage storage, IKeySerializer<TKey> keySerializer, bool isNew)
        {
            _collectionName = collectionName;
            _collectionId = collectionId;
            _storage = storage;
            _keySerializer = keySerializer;
            _isNew = isNew;

            _minKey = FillBinKey(byte.MinValue);
            _maxKey = FillBinKey(byte.MaxValue);
        }

        public string Name => _collectionName;
        public long ByteSize => GetSize();

        public IEnumerable<KeyValuePair<TKey, ArraySegment<byte>>> Iterate(TKey from, bool reversed)
        {
            byte[] refKey = GetBinKey(from);

            using (var dbIterator = _storage.Database.CreateIterator())
            {
                Seek(dbIterator, refKey.ToArray(), reversed);

                while (true)
                {
                    if (!dbIterator.Valid())
                        yield break; // end of db

                    TKey key;

                    if (!TryGetKey(dbIterator.Key(), out key))
                        yield break; // end of collection

                    yield return new KeyValuePair<TKey, ArraySegment<byte>>(key, new ArraySegment<byte>(dbIterator.Value()));

                    if (reversed)
                        dbIterator.Prev();
                    else
                        dbIterator.Next();
                }
            }
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed)
        {
            byte[] refKey = GetBinKey(from);

            using (var dbIterator = _storage.Database.CreateIterator())
            {
                Seek(dbIterator, refKey, reversed);

                while (true)
                {
                    if (!dbIterator.Valid())
                        yield break; // end of db

                    TKey key;

                    if (!TryGetKey(dbIterator.Key(), out key))
                        yield break; // end of collection

                    yield return key;

                    if (reversed)
                        dbIterator.Prev();
                    else
                        dbIterator.Next();
                }
            }
        }

        public bool Read(TKey key, out ArraySegment<byte> value)
        {
            var binKey = GetBinKey(key);
            value = new ArraySegment<byte>(_storage.Database.Get(binKey));
            return value != null;
        }

        public void Write(TKey key, ArraySegment<byte> value)
        {
            EnsureNameHeaderWritten();
            var binKey = GetBinKey(key);
            // TO DO : ToArray() causes bad performance
            _storage.Database.Put(binKey, value.ToArray());
        }

        public void Remove(TKey key)
        {
            var binKey = GetBinKey(key);
            _storage.Database.Delete(binKey);
        }

        private void RemoveRange(byte[] fromKey, byte[] toKey)
        {
            using (var dbIterator = _storage.Database.CreateIterator())
            {
                Seek(dbIterator, fromKey);

                while (true)
                {
                    if (!dbIterator.Valid())
                        return; // end of db

                    TKey key;

                    if (!TryGetKey(dbIterator.Key(), out key))
                        return; // end of collection

                    _storage.Database.Delete(dbIterator.Key());

                    dbIterator.Next();
                }
            }
        }

        public void RemoveAll()
        {
            RemoveRange(_minKey, _maxKey);
        }

        public void Compact()
        {
            _storage.Database.CompactRange(_minKey, _maxKey);
        }

        public long GetSize()
        {
            return _storage.Database.GetApproximateSize(_minKey, _maxKey);
        }

        public void Drop()
        {
            RemoveAll();
            Compact();
            _storage.OnDropped(this);
            // TO DO : Drop header
            _isDisposed = true;
        }

        private void Seek(LevelDB.Iterator dbIterator, byte[] refKey, bool reversed = false)
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

                if (!reversed)
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
            _storage.Database.Put(h.GetBinaryKey(), h.Content);
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
