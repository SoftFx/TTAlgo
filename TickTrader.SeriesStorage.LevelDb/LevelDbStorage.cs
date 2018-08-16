using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.LevelDb
{
    public class LevelDbStorage : IKeyValueBinaryStorage
    {
        private LevelDB.DB _database;

        public bool SupportsByteSize => true;
        public bool SupportsRemoveAll => false;

        public LevelDbStorage(string name)
        {
            var options = new LevelDB.Options { CreateIfMissing = true };
            _database = new LevelDB.DB(name, options);
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> Iterate(bool reversed)
        {
            using (var dbIterator = _database.CreateIterator())
            {
                if (reversed)
                    dbIterator.SeekToLast();

                while (dbIterator.Valid())
                {
                    yield return new KeyValuePair<byte[], byte[]>(dbIterator.Key(), dbIterator.Value());

                    if (reversed)
                        dbIterator.Prev();
                    else
                        dbIterator.Next();
                }
            }
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> Iterate(byte[] seek, bool reversed)
        {
            using (var dbIterator = _database.CreateIterator())
            {
                SeekNear(dbIterator, seek, reversed);

                while (dbIterator.Valid())
                {
                    var key = dbIterator.Key();

                    yield return new KeyValuePair<byte[], byte[]>(key, dbIterator.Value());

                    if (reversed)
                        dbIterator.Prev();
                    else
                        dbIterator.Next();
                }
            }
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> Iterate(byte[] seek, byte[] from, byte[] to, bool reversed)
        {
            using (var dbIterator = _database.CreateIterator())
            {
                SeekNear(dbIterator, seek, from, to, reversed);

                while (dbIterator.Valid())
                {
                    var key = dbIterator.Key();

                    if (KeyHelper.IsGreater(key, to))
                        yield break; // end of collection

                    yield return new KeyValuePair<byte[], byte[]>(key, dbIterator.Value());

                    if (reversed)
                        dbIterator.Prev();
                    else
                        dbIterator.Next();
                }
            }
        }

        public IEnumerable<byte[]> IterateKeys(bool reversed = false)
        {
            using (var dbIterator = _database.CreateIterator())
            {
                if (reversed)
                    dbIterator.SeekToLast();

                while (dbIterator.Valid())
                {
                    var key = dbIterator.Key();

                    yield return key;

                    if (reversed)
                        dbIterator.Prev();
                    else
                        dbIterator.Next();
                }
            }
        }

        public IEnumerable<byte[]> IterateKeys(byte[] seek, bool reversed = false)
        {
            using (var dbIterator = _database.CreateIterator())
            {
                SeekNear(dbIterator, seek, reversed);

                while (dbIterator.Valid())
                {
                    var key = dbIterator.Key();

                    yield return key;

                    if (reversed)
                        dbIterator.Prev();
                    else
                        dbIterator.Next();
                }
            }
        }

        public IEnumerable<byte[]> IterateKeys(byte[] seek, byte[] from, byte[] to, bool reversed = false)
        {
            using (var dbIterator = _database.CreateIterator())
            {
                SeekNear(dbIterator, seek, from, to, reversed);

                while (dbIterator.Valid())
                {
                    var key = dbIterator.Key();

                    if (KeyHelper.IsGreater(key, to))
                        yield break; // end of collection

                    yield return key;

                    if (reversed)
                        dbIterator.Prev();
                    else
                        dbIterator.Next();
                }
            }
        }

        public bool Read(byte[] key, out byte[] value)
        {
            value = _database.Get(key);
            return key != null;
        }

        public void Write(byte[] key, byte[] value)
        {
            _database.Put(key, value);
        }

        public void Remove(byte[] key)
        {
            _database.Delete(key);
        }

        public void RemoveRange(byte[] from, byte[] to)
        {
            using (var dbIterator = _database.CreateIterator())
            {
                dbIterator.Seek(from);

                while (true)
                {
                    if (!dbIterator.Valid())
                        return; // end of db

                    var key = dbIterator.Key();

                    if (KeyHelper.IsGreater(key, to))
                        return; // end of collection

                    _database.Delete(dbIterator.Key());

                    dbIterator.Next();
                }
            }
        }

        public void RemoveAll()
        {
            // RemoveAll() is not supported
            throw new NotSupportedException();
        }

        public void CompactRange(byte[] from, byte[] to)
        {
            _database.CompactRange(from, to);
        }

        public long GetSize()
        {
            // GetSize() is not supported
            throw new NotSupportedException();
        }

        public long GetSize(byte[] from, byte[] to)
        {
            return _database.GetApproximateSize(from, to);
        }

        private void SeekNear(LevelDB.Iterator dbIterator, byte[] seekNear, bool reversed = false)
        {
            dbIterator.Seek(seekNear);

            if (dbIterator.Valid())
            {
                var iKey = dbIterator.Key();
                if (Enumerable.SequenceEqual(iKey, seekNear))
                    return; // exact position

                // try step back
                dbIterator.Prev();

                if (!dbIterator.Valid())
                    dbIterator.Next();  // revert stepping back
            }
            else
            {
                // I have to use a hack :(
                // the only case seems to be end of base
                dbIterator.SeekToLast();
            }
        }

        private void SeekNear(LevelDB.Iterator dbIterator, byte[] seekNear, byte[] min, byte[] max, bool reversed = false)
        {
            dbIterator.Seek(seekNear);

            if (dbIterator.Valid())
            {
                var iKey = dbIterator.Key();
                if (Enumerable.SequenceEqual(iKey, seekNear))
                    return; // exact position

                // try step back
                dbIterator.Prev();

                if (dbIterator.Valid())
                {
                    //if (TryGetKey(dbIterator.Key(), out key))
                    if(KeyHelper.IsGreaterOrEqual(dbIterator.Key(), min))
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
    }
}
