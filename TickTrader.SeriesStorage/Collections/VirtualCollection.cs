using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IEnumerable<KeyValuePair<TKey, ArraySegment<byte>>> Iterate(bool reversed)
        {
            byte[] fromKey = reversed ? _maxKey : _minKey;
            byte[] toKey = reversed ? _minKey : _maxKey;

            return _emulator.Database.Iterate(fromKey, fromKey, toKey, reversed)
                .Select(p => new KeyValuePair<TKey, ArraySegment<byte>>(GetKey(p.Key), new ArraySegment<byte>(p.Value)));
        }

        public IEnumerable<KeyValuePair<TKey, ArraySegment<byte>>> Iterate(TKey from, bool reversed)
        {
            byte[] seekKey = GetBinKey(from);
            byte[] fromKey = reversed ? _maxKey : _minKey;
            byte[] toKey = reversed ? _minKey : _maxKey;

            return _emulator.Database.Iterate(seekKey, fromKey, toKey, reversed)
                .Select(p => new KeyValuePair<TKey, ArraySegment<byte>>(GetKey(p.Key), new ArraySegment<byte>(p.Value)));
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed)
        {
            byte[] seekKey = GetBinKey(from);
            byte[] fromKey = reversed ? _maxKey : _minKey;
            byte[] toKey = reversed ? _minKey : _maxKey;

            return _emulator.Database.IterateKeys(fromKey, fromKey, toKey, reversed)
                .Select(binKey => GetKey(binKey));
        }

        public bool Read(TKey key, out ArraySegment<byte> value)
        {
            var binKey = GetBinKey(key);
            byte[] binValue;
            _emulator.Database.Read(binKey, out binValue);
            value = (binValue == null) ? new ArraySegment<byte>() : new ArraySegment<byte>(binValue);
            return binValue != null;
        }

        public void Write(TKey key, ArraySegment<byte> value)
        {
            EnsureNameHeaderWritten();
            var binKey = GetBinKey(key);
            // TO DO : ToArray() causes bad performance
            _emulator.Database.Write(binKey, value.ToArray());
        }

        public void Remove(TKey key)
        {
            var binKey = GetBinKey(key);
            _emulator.Database.Remove(binKey);
        }

        public void RemoveRange(TKey from, TKey to)
        {
            _emulator.Database.RemoveRange(GetBinKey(from), GetBinKey(to));
        }

        public void RemoveAll()
        {
            if (_emulator.Database.SupportsRemoveAll)
                _emulator.Database.RemoveAll();
            else
                _emulator.Database.RemoveRange(_maxKey, _maxKey);
        }

        public void Compact()
        {
            _emulator.Database.CompactRange(_minKey, _maxKey);
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

        //private void Seek(LevelDB.Iterator dbIterator, byte[] refKey, bool reversed = false)
        //{
        //    dbIterator.Seek(refKey);

        //    if (dbIterator.Valid())
        //    {
        //        var iKey = dbIterator.Key();
        //        if (Enumerable.SequenceEqual(iKey, refKey))
        //            return; // exact position

        //        // try step back
        //        dbIterator.Prev();

        //        if (dbIterator.Valid())
        //        {
        //            TKey key;

        //            if (TryGetKey(dbIterator.Key(), out key))
        //                return; // prev key is from this collection 
        //        }

        //        if (!reversed)
        //            dbIterator.Next();  // revert stepping back
        //    }
        //    else
        //    {
        //        // I have to use a hack :(
        //        // the only case seems to be end of base
        //        dbIterator.SeekToLast();
        //    }
        //}

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

        private TKey GetKey(byte[] binKey)
        {
            var reader = new BinaryKeyReader(binKey);
            var collectionId = reader.ReadBeUshort();
            if (collectionId != _collectionId)
                throw new Exception("Invalid key!");

            return _keySerializer.Deserialize(reader);
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
            _emulator.Database.Write(h.GetBinaryKey(), h.Content);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Collection");
        }
    }
}
