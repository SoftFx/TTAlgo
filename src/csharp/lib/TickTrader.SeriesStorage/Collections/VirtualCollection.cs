using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TickTrader.SeriesStorage
{
    internal class VirtualCollection<TKey> : IBinaryStorageCollection<TKey>, IBinaryCollection
    {
        private ushort _collectionId;
        private string _collectionName;
        private CollectionEmulator _emulator;
        private IKeySerializer<TKey> _keySerializer;
        private bool _isNew;
        private bool _isDisposed;

        private byte[] _minKey;
        private byte[] _maxKey;

        public VirtualCollection(string collectionName, ushort collectionId, CollectionEmulator emulator, IKeySerializer<TKey> keySerializer, bool isNew)
        {
            _collectionName = collectionName;
            _collectionId = collectionId;
            _emulator = emulator;
            _keySerializer = keySerializer;
            _isNew = isNew;

            _minKey = FillBinKey(byte.MinValue);
            _maxKey = FillBinKey(byte.MaxValue);
        }

        public string Name => _collectionName;
        public long ByteSize => GetSize();

        public ITransaction StartTransaction()
        {
            return _emulator.Database.StartTransaction();
        }

        public IEnumerable<KeyValuePair<TKey, ArraySegment<byte>>> Iterate(bool reversed, ITransaction transaction = null)
        {
            using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
            {
                using (var dbIterator = _emulator.Database.CreateCursor(trScope.Transaction))
                {
                    if (reversed)
                        dbIterator.SeekTo(_maxKey);
                    else
                        dbIterator.SeekTo(_minKey);

                    while (true)
                    {
                        if (!dbIterator.IsValid)
                            break; // end of db

                        TKey key;

                        if (!TryGetKey(dbIterator.GetKey(), out key))
                            break; // end of collection

                        yield return new KeyValuePair<TKey, ArraySegment<byte>>(key, new ArraySegment<byte>(dbIterator.GetValue()));

                        if (reversed)
                            dbIterator.MoveToPrev();
                        else
                            dbIterator.MoveToNext();
                    }
                }

                trScope.Commit();
            }
        }

        public IEnumerable<KeyValuePair<TKey, ArraySegment<byte>>> Iterate(TKey from, bool reversed, ITransaction transaction = null)
        {
            byte[] refKey = GetBinKey(from);

            using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
            {
                using (var dbIterator = _emulator.Database.CreateCursor(trScope.Transaction))
                {
                    Seek(dbIterator, refKey.ToArray(), reversed);

                    while (true)
                    {
                        if (!dbIterator.IsValid)
                            break; // end of db

                        TKey key;

                        if (!TryGetKey(dbIterator.GetKey(), out key))
                            break; // end of collection

                        yield return new KeyValuePair<TKey, ArraySegment<byte>>(key, new ArraySegment<byte>(dbIterator.GetValue()));

                        if (reversed)
                            dbIterator.MoveToPrev();
                        else
                            dbIterator.MoveToNext();
                    }
                }

                trScope.Commit();
            }
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed, ITransaction transaction = null)
        {
            byte[] refKey = GetBinKey(from);

            using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
            {
                using (var dbIterator = _emulator.Database.CreateCursor(trScope.Transaction))
                {
                    Seek(dbIterator, refKey.ToArray(), reversed);

                    while (true)
                    {
                        if (!dbIterator.IsValid)
                            break; // end of db

                        TKey key;

                        if (!TryGetKey(dbIterator.GetKey(), out key))
                            break; // end of collection

                        yield return key;

                        if (reversed)
                            dbIterator.MoveToPrev();
                        else
                            dbIterator.MoveToNext();
                    }

                    trScope.Commit();
                }
            }
        }

        public bool Read(TKey key, out ArraySegment<byte> value, ITransaction transaction = null)
        {
            var binKey = GetBinKey(key);
            byte[] binValue;
            using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
            {
                _emulator.Database.Read(binKey, out binValue, trScope.Transaction);
                trScope.Commit();
            }
            value = (binValue == null) ? new ArraySegment<byte>() : new ArraySegment<byte>(binValue);
            return binValue != null;
        }

        public void Write(TKey key, ArraySegment<byte> value, ITransaction transaction = null)
        {
            EnsureNameHeaderWritten();
            var binKey = GetBinKey(key);
            // TO DO : ToArray() causes bad performance
            using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
            {
                _emulator.Database.Write(binKey, value.ToArray(), trScope.Transaction);
                trScope.Commit();
            }
        }

        public void Remove(TKey key, ITransaction transaction = null)
        {
            var binKey = GetBinKey(key);
            using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
            {
                _emulator.Database.Remove(binKey, trScope.Transaction);
                trScope.Commit();
            }
        }

        public void RemoveRange(TKey from, TKey to, ITransaction transaction = null)
        {
            using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
            {
                _emulator.Database.RemoveRange(GetBinKey(from), GetBinKey(to), trScope.Transaction);
                trScope.Commit();
            }
        }

        public void RemoveAll(ITransaction transaction = null)
        {
            using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
            {
                if (_emulator.Database.SupportsRemoveAll)
                    _emulator.Database.RemoveAll(trScope.Transaction);
                else
                    _emulator.Database.RemoveRange(_maxKey, _maxKey, trScope.Transaction);

                trScope.Commit();
            }
        }

        public void Compact()
        {
            using (var trScope = new AutoTransactionScope(null, StartTransaction))
            {
                _emulator.Database.CompactRange(_minKey, _maxKey, trScope.Transaction);
                trScope.Commit();
            }
        }

        public long GetSize()
        {
            return _emulator.Database.GetSize(_minKey, _maxKey);
        }

        public void Drop()
        {
            RemoveAll();
            Compact();
            _emulator.OnDropped(this);
            // TO DO : Drop header
            _isDisposed = true;
        }

        public void Dispose()
        {
            _isDisposed = true;
        }

        private void Seek(IKeyValueBinaryCursor dbIterator, byte[] refKey, bool reversed = false)
        {
            dbIterator.SeekTo(refKey);

            if (dbIterator.IsValid)
            {
                var iKey = dbIterator.GetKey();
                if (Enumerable.SequenceEqual(iKey, refKey))
                    return; // exact position

                // try step back
                dbIterator.MoveToPrev();

                if (dbIterator.IsValid)
                {
                    TKey key;

                    if (TryGetKey(dbIterator.GetKey(), out key))
                        return; // prev key is from this collection 
                }

                if (!reversed)
                    dbIterator.MoveToNext();  // revert stepping back
            }
            else
            {
                // end of base
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
                var nameHeader = new CollectionHeader(_collectionId, HeaderTypes.TableName, Encoding.UTF8.GetBytes(_collectionName));
                WriteHeader(nameHeader);
                _isNew = false;
            }
        }

        private void WriteHeader(CollectionHeader h)
        {
            using (var tr = StartTransaction())
            {
                _emulator.Database.Write(h.GetBinaryKey(), h.Content, tr);
                tr.Commit();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Collection");
        }
    }
}
