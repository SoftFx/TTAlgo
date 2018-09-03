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

        public bool SupportsRemoveAll => false;
        public bool SupportsCursorRemove => false;
        public bool SupportsCompaction => true;

        public LevelDbStorage(string name)
        {
            var options = new LevelDB.Options { CreateIfMissing = true };
            _database = new LevelDB.DB(name, options);
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        public IKeyValueBinaryCursor CreateCursor()
        {
            return new LevelDbCursor(_database.CreateIterator());
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
    }
}
