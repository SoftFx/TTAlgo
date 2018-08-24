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

        public LmdbStorage(string path, bool readOnly = false)
        {
            _env = new LightningEnvironment(path);
            _readonlyMode = readOnly;
            try
            {
                _env.MaxDatabases = 1000;
                if (Environment.Is64BitProcess)
                    _env.MapSize = 1024L * 1024L * 1024L * 50L;
                else
                    _env.MapSize = 1024L * 1024L * 1024L;
                _env.MaxReaders = 1000;

                var flags = EnvironmentOpenFlags.WriteMap | EnvironmentOpenFlags.NoSync | EnvironmentOpenFlags.NoSubDir;

                if (readOnly)
                    flags |= EnvironmentOpenFlags.ReadOnly;

                _env.Open(flags);
            }
            catch
            {
                _env.Dispose();
                _env = null;
                throw;
            }
        }

        public void Dispose()
        {
            //_env.Flush(true);
            _env.Dispose();
        }

        public IKeyValueBinaryCursor CreateCursor()
        {
            return new LmdbCursor(_env, DefaultDatabaseName, defaultDbCfg, _readonlyMode);
        }

        public bool Read(byte[] key, out byte[] value)
        {
            using (var tr = _env.BeginTransaction())
            {
                var db = tr.OpenDatabase(DefaultDatabaseName, defaultDbCfg);
                return tr.TryGet(db, key, out value);
            }
        }

        public void Write(byte[] key, byte[] value)
        {
            using (var tr = _env.BeginTransaction())
            {
                var db = tr.OpenDatabase(DefaultDatabaseName, defaultDbCfg);
                tr.Put(db, key, value);
                tr.Commit();
            }
        }

        public void Remove(byte[] key)
        {
            using (var tr = _env.BeginTransaction())
            {
                var db = tr.OpenDatabase(DefaultDatabaseName, defaultDbCfg);
                tr.Delete(db, key);
                tr.Commit();
            }
        }

        public void RemoveRange(byte[] from, byte[] to)
        {
            using (var tr = _env.BeginTransaction())
            {
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

                tr.Commit();
            }
        }

        public void RemoveAll()
        {
            using (var tr = _env.BeginTransaction())
            {
                var db = tr.OpenDatabase(DefaultDatabaseName, defaultDbCfg);
                db.Truncate(tr);
                tr.Commit();
            }
        }

        public void CompactRange(byte[] from, byte[] to)
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
