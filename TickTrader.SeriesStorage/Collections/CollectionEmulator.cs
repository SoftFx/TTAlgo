using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    internal class CollectionEmulator : ISeriesDatabase
    {
        private IKeyValueBinaryStorage _binStorage;

        private Dictionary<ushort, string> _idToNameMap = new Dictionary<ushort, string>();
        private Dictionary<string, ushort> _nameToIdMap = new Dictionary<string, ushort>();
        private Dictionary<string, IBinaryCollection> _collections = new Dictionary<string, IBinaryCollection>();

        public bool SupportsByteSize => true;
        public IEnumerable<string> Collections { get => _idToNameMap.Values; }
        public IKeyValueBinaryStorage Database => _binStorage;

        public CollectionEmulator(IKeyValueBinaryStorage binSorage)
        {
            _binStorage = binSorage;
            Init();
        }

        public IBinaryStorageCollection<TKey> GetBinaryCollection<TKey>(string name, IKeySerializer<TKey> keySerializer)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException();

            IBinaryCollection collection;

            if (_collections.TryGetValue(name, out collection))
            {
                if (collection is VirtualCollection<TKey>)
                    return (VirtualCollection<TKey>)collection;
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

            var collection = new VirtualCollection<TKey>(name, collectionId, this, keySerializer, isNew);
            _collections.Add(name, collection);
            return collection;
        }

        internal void OnDropped(IBinaryCollection collection)
        {
            _collections.Remove(collection.Name);
        }

        private void Init()
        {
            using (var cursor = _binStorage.CreateCursor(null))
            {
                cursor.SeekToFirst();

                while (cursor.IsValid)
                {
                    var record = cursor.GetRecord();

                    if (!CollectionHeader.IsNameRecord(record))
                        break;

                    var nameHeader = new CollectionHeader(record);
                    var collectionName = Encoding.UTF8.GetString(nameHeader.Content);
                    AddCollectionMapping(nameHeader.CollectionId, collectionName);

                    cursor.MoveToNext();
                }

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
            _binStorage.Dispose();
        }

        public static void Destory(string path)
        {
            LevelDB.DB.Destroy(path);
        }
    }
}
