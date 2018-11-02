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

        public ITransaction StartTransaction()
        {
            return Storage.StartTransaction();
        }

        public virtual void Dispose()
        {
            Storage.Dispose();
        }

        public virtual void Drop()
        {
            throw new NotSupportedException();
        }

        public virtual long GetSize()
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

        public IEnumerable<KeyValuePair<TKey, ArraySegment<byte>>> Iterate(bool reversed = false, ITransaction transaction = null)
        {
            OnStartAccess();
            try
            {
                using (var dbIterator = Storage.CreateCursor(transaction))
                {
                    if (reversed)
                        dbIterator.SeekToLast();
                    else
                        dbIterator.SeekToFirst();

                    while (dbIterator.IsValid)
                    {                        
                        TKey key = GetTypedKey(dbIterator.GetKey());

                        yield return new KeyValuePair<TKey, ArraySegment<byte>>(key, new ArraySegment<byte>(dbIterator.GetValue()));

                        if (reversed)
                            dbIterator.MoveToPrev();
                        else
                            dbIterator.MoveToNext();
                    }
                }
            }
            finally
            {
                OnStopAccess();
            }
        }

        public IEnumerable<KeyValuePair<TKey, ArraySegment<byte>>> Iterate(TKey from, bool reversed = false, ITransaction transaction = null)
        {
            OnStartAccess();
            try
            {
                byte[] refKey = GetBinKey(from);

                using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
                {
                    using (var dbIterator = Storage.CreateCursor(trScope.Transaction))
                    {
                        Seek(dbIterator, refKey.ToArray(), reversed);

                        while (true)
                        {
                            if (!dbIterator.IsValid)
                                yield break; // end of db

                            TKey key = GetTypedKey(dbIterator.GetKey());

                            yield return new KeyValuePair<TKey, ArraySegment<byte>>(key, new ArraySegment<byte>(dbIterator.GetValue()));

                            if (reversed)
                                dbIterator.MoveToPrev();
                            else
                                dbIterator.MoveToNext();
                        }
                    }
                }
            }
            finally
            {
                OnStopAccess();
            }
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed = false, ITransaction transaction = null)
        {
            OnStartAccess();
            try
            {
                byte[] refKey = GetBinKey(from);

                using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
                {
                    using (var dbIterator = Storage.CreateCursor(trScope.Transaction))
                    {
                        Seek(dbIterator, refKey.ToArray(), reversed);

                        while (true)
                        {
                            if (!dbIterator.IsValid)
                                yield break; // end of db

                            yield return GetTypedKey(dbIterator.GetKey());

                            if (reversed)
                                dbIterator.MoveToPrev();
                            else
                                dbIterator.MoveToNext();
                        }
                    }
                }
            }
            finally
            {
                OnStopAccess();
            }
        }

        public bool Read(TKey key, out ArraySegment<byte> value, ITransaction transaction = null)
        {
            OnStartAccess();
            try
            {
                using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
                {
                    byte[] binValue;
                    if (Storage.Read(GetBinKey(key), out binValue, trScope.Transaction))
                    {
                        value = new ArraySegment<byte>(binValue);
                        return true;
                    }
                    value = new ArraySegment<byte>();
                    return false;
                }
            }
            finally
            {
                OnStopAccess();
            }
        }

        public void Remove(TKey key, ITransaction transaction = null)
        {
            OnStartAccess();
            try
            {
                using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
                {
                    Storage.Remove(GetBinKey(key), trScope.Transaction);
                    trScope.Commit();
                }
            }
            finally
            {
                OnStopAccess();
            }
        }

        public void RemoveAll(ITransaction transaction = null)
        {
            OnStartAccess();
            try
            {
                using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
                {
                    Storage.RemoveAll(trScope.Transaction);
                    trScope.Commit();
                }
            }
            finally
            {
                OnStopAccess();
            }   
        }

        public void RemoveRange(TKey from, TKey to, ITransaction transaction = null)
        {
            OnStartAccess();
            try
            {
                using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
                {
                    Storage.RemoveRange(GetBinKey(from), GetBinKey(to), trScope.Transaction);
                    trScope.Commit();
                }
            }
            finally
            {
                OnStopAccess();
            }
        }

        public void Write(TKey key, ArraySegment<byte> value, ITransaction transaction = null)
        {
            OnStartAccess();
            try
            {
                using (var trScope = new AutoTransactionScope(transaction, StartTransaction))
                {
                    Storage.Write(GetBinKey(key), value.ToArray(), trScope.Transaction);
                    trScope.Commit();
                }
            }
            finally
            {
                OnStopAccess();
            }
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

                if (!dbIterator.IsValid)
                {
                    if(!reversed)
                        dbIterator.SeekToFirst(); // revert stepping back
                }
            }
            else
            {
                // end of base
                dbIterator.SeekToLast();
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
