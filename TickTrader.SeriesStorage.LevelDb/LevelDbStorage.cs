using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.LevelDb
{
    public class LevelDbStorage : IMulticollectionBinaryStorage
    {
        private LevelDB.DB _database;
        private Dictionary<ushort, string> _idToNameMap = new Dictionary<ushort, string>();
        private Dictionary<string, ushort> _nameToIdMap = new Dictionary<string, ushort>();
        private Dictionary<string, IBinaryCollection> _collections = new Dictionary<string, IBinaryCollection>();

        public bool SupportsByteSize => true;
        public IEnumerable<string> Collections { get => _idToNameMap.Values; }

        public LevelDbStorage(string name)
        {
            var options = new LevelDB.Options { CreateIfMissing = true };
            _database = new LevelDB.DB(name, options);
            try
            {
                Init();
            }
            catch
            {
                _database.Dispose();
                throw;
            }
        }

        public IBinaryStorageCollection<TKey> GetBinaryCollection<TKey>(string name, IKeySerializer<TKey> keySerializer)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException();

            IBinaryCollection collection;

            if (_collections.TryGetValue(name, out collection))
            {
                if (collection is LevelDbCollection<TKey>)
                    return (LevelDbCollection<TKey>)collection;
                throw new Exception("Collection with same name and different key type alredy exist!");
            }

            return AddCollection<TKey>(name, keySerializer);
        }

        private IBinaryStorageCollection<TKey> AddCollection<TKey>(string name, IKeySerializer<TKey> keySerializer)
        {
            ushort collectionId;
            bool isNew = false;

            if (!_nameToIdMap.TryGetValue(name, out collectionId))
            {
                collectionId = CreateNewMapping(name);
                isNew = true;
            }

            var collection = new LevelDbCollection<TKey>(name, collectionId, _database, keySerializer, isNew);
            _collections.Add(name, collection);
            return collection;
        }

        private void Init()
        {
            foreach (var record in _database)
            {
                if (!Header.IsNameRecord(record))
                    break;

                var nameHeader = new Header(record);
                var collectionName = Encoding.UTF8.GetString(nameHeader.Content);
                AddCollectionMapping(nameHeader.CollectionId, collectionName);
            }
        }

        private void AddCollectionMapping(ushort id, string name)
        {
            _idToNameMap.Add(id, name);
            _nameToIdMap.Add(name, id);
        }

        private void RemoveCollectionMapping(ushort id, string name)
        {
            _idToNameMap.Remove(id);
            _nameToIdMap.Remove(name);
        }

        private ushort CreateNewMapping(string name)
        {
            for (ushort i = 1; i < ushort.MaxValue; i++)
            {
                if (!_idToNameMap.ContainsKey(i))
                {
                    AddCollectionMapping(i, name);
                    return i;
                }
            }
            throw new Exception("Maximum numbe of collections is reached!");
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        public static void Destory(string path)
        {
            LevelDB.DB.Destroy(path);
        }
    }
}
