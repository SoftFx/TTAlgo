using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    internal class DatabasePool : ISeriesDatabase
    {
        private IBinaryStorageManager _manager;
        private Dictionary<string, IPooledDatabase> _databases = new Dictionary<string, IPooledDatabase>();

        public DatabasePool(IBinaryStorageManager manager)
        {
            _manager = manager;
        }

        public IEnumerable<string> Collections => _manager.GetStorages();

        public void Dispose()
        {
            lock (_databases)
            {
                foreach (var db in _databases.Values)
                    db.Close();
            }
        }

        public IBinaryStorageCollection<TKey> GetBinaryCollection<TKey>(string storageName, IKeySerializer<TKey> keySerializer)
        {
            lock (_databases)
            {
                IPooledDatabase untypedCollection;
                if (_databases.TryGetValue(storageName, out untypedCollection))
                {
                    if (untypedCollection is Database<TKey>)
                        return (VirtualCollection<TKey>)untypedCollection;
                    throw new Exception("Collection with same name and different key type alredy exist!");
                }

                var newDb = new Database<TKey>(storageName, _manager, keySerializer);
                _databases.Add(storageName, newDb);
                return newDb;
            }
        }

        private interface IPooledDatabase
        {
            void Close();
        }

        private class Database<TKey> : CollectionAdapter<TKey>, IPooledDatabase
        {
            private object _accessLock = new object();
            private IBinaryStorageManager _manager;
            private int _accessCount;
            private bool _isClosed;

            public Database(string name, IBinaryStorageManager manager, IKeySerializer<TKey> keySerializer)
                : base(name, null, keySerializer)
            {
                _manager = manager;
            }

            public void Close()
            {
                lock (_accessLock)
                    _isClosed = true;
            }

            public override void Dispose()
            {
                // do nothing
                //throw new Exception("A database form database pool cannot be disposed directly!");
            }

            protected override void OnStartAccess()
            {
                lock (_accessLock)
                {
                    if (_isClosed)
                        throw new InvalidOperationException("Database has been closed!");

                    _accessCount++;

                    if (Storage == null)
                        Storage = _manager.OpenStorage(Name);
                }
            }

            protected override void OnStopAccess()
            {
                lock (_accessLock)
                {
                    _accessCount--;

                    if (_accessCount <= 0 && Storage != null)
                    {
                        Storage.Dispose();
                        Storage = null;
                    }
                }
            }
        }
    }
}
