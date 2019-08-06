using LightningDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.Lmdb
{
    public class LmdbStorage : IKeyValueBinaryStorage
    {
        public const string DefaultDatabaseName = "default";

        private static DatabaseConfiguration defaultDbCfg = new DatabaseConfiguration() { Flags = DatabaseOpenFlags.Create };

        private LightningEnvironment _env;
        private bool _readonlyMode;
        //private Dictionary<ushort, string> _idToNameMap = new Dictionary<ushort, string>();
        //private Dictionary<string, ushort> _nameToIdMap = new Dictionary<string, ushort>();
        //private Dictionary<string, IBinaryCollection> _collections = new Dictionary<string, IBinaryCollection>();

        public bool SupportsRemoveAll => true;
        public bool SupportsCursorRemove => true;
        public bool SupportsCompaction => false;

        public LmdbStorage(string path, bool readOnly = false)
        {
            _env = new LightningEnvironment(path);
            _readonlyMode = readOnly;
            try
            {
                _env.MaxDatabases = 100000;
                if (Environment.Is64BitProcess)
                    _env.MapSize = 1024L * 1024L * 1024L * 50L;
                else
                    _env.MapSize = 1024L * 1024L * 1024L;
                _env.MaxReaders = 100000;

                var flags = EnvironmentOpenFlags.WriteMap | EnvironmentOpenFlags.NoSync | EnvironmentOpenFlags.NoSubDir;

                if (readOnly)
                    flags |= EnvironmentOpenFlags.ReadOnly;

                _env.Open(flags);
            }
            catch(Exception ex)
            {
                _env.Dispose();
                _env = null;
                throw LmdbException.Convert(ex);
            }
        }

        public void Dispose()
        {
            //_env.Flush(true);
            _env.Dispose();
        }

        public ITransaction StartTransaction()
        {
            return new LmdbTransaction(_env, _readonlyMode);
        }

        public IKeyValueBinaryCursor CreateCursor(ITransaction transaction)
        {
            var tr = ((LmdbTransaction)transaction).Handle;
            return new LmdbCursor(tr, DefaultDatabaseName, defaultDbCfg, _readonlyMode);
        }

        public bool Read(byte[] key, out byte[] value, ITransaction transaction)
        {
            var tr = ((LmdbTransaction)transaction).Handle;
            var db = tr.OpenDatabase(DefaultDatabaseName, defaultDbCfg);
            var result = tr.TryGet(db, key, out value);

            return result;
        }

        public void Write(byte[] key, byte[] value, ITransaction transaction)
        {
            var tr = ((LmdbTransaction)transaction).Handle;
            var db = tr.OpenDatabase(DefaultDatabaseName, defaultDbCfg);
            tr.Put(db, key, value);
        }

        public void Remove(byte[] key, ITransaction transaction)
        {
            var tr = ((LmdbTransaction)transaction).Handle;
            var db = tr.OpenDatabase(DefaultDatabaseName, defaultDbCfg);
            tr.Delete(db, key);
        }

        public void RemoveRange(byte[] from, byte[] to, ITransaction transaction)
        {
            var tr = ((LmdbTransaction)transaction).Handle;

            var db = tr.OpenDatabase(DefaultDatabaseName, defaultDbCfg);
            var cursor = tr.CreateCursor(db);

            if (cursor.MoveTo(from))
                return;

            while (cursor.MoveNext())
            {
                var key = cursor.Current.Key;

                if (KeyHelper.IsLess(key, to))
                    break;

                cursor.Delete();
            }
        }

        public void RemoveAll(ITransaction transaction)
        {
            var tr = ((LmdbTransaction)transaction).Handle;
            var db = tr.OpenDatabase(DefaultDatabaseName, defaultDbCfg);
            db.Truncate(tr);
        }

        public void CompactRange(byte[] from, byte[] to, ITransaction transaction)
        {
            throw new NotSupportedException();
        }

        public long GetSize()
        {
            return 0;
        }

        public long GetSize(byte[] from, byte[] to)
        {
            throw new NotSupportedException();
        }

        private bool SeekNear(LightningCursor cursor, byte[] key, bool reversed = false)
        {
            if (cursor.MoveTo(key))
            {
                var iKey = cursor.Current.Key;
                if (Enumerable.SequenceEqual(iKey, key))
                    return true; // exact position
                else
                {
                    if (!reversed)
                        cursor.MovePrev();
                    return true;
                }
            }
            else
                return false;
        }
    }
}
