using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.LevelDb;
using TickTrader.SeriesStorage.Protobuf;

namespace TickTrader.Algo.Common.Model
{
    public class CustomFeedStorage : FeedCache
    {
        private ICollectionStorage<Guid, CustomSymbol> _metadataStorage;
        private DynamicDictionary<string, CustomSymbolModel> _symbols = new DynamicDictionary<string, CustomSymbolModel>();

        public CustomFeedStorage(string dbFolder) : base(dbFolder)
        {
        }

        public override void Start()
        {
            lock (SyncObj)
            {
                base.Start();

                _metadataStorage = Database.GetCollection("symbols", new GuidKeySerializer(), new ProtoValueSerializer<CustomSymbol>());

                foreach (var entry in _metadataStorage.Iterate(Guid.Empty))
                    _symbols.Add(entry.Value.Name, new CustomSymbolModel(entry.Key, entry.Value));
            }
        }

        protected override bool IsSpecialCollection(string name)
        {
            return name == "symbols";
        }

        public override void Stop()
        {
            lock (SyncObj)
            {
                _metadataStorage.Dispose();
                _metadataStorage = null;

                base.Stop();
            }
        }

        public IDynamicDictionarySource<string, CustomSymbolModel> GetSymbolsSyncCopy(ISyncContext context)
        {
            lock (SyncObj) return new DictionarySyncrhonizer<string, CustomSymbolModel>(_symbols, context);
        }

        public Task AddAsync(CustomSymbol newSymbol)
        {
            return Task.Factory.StartNew(() => Add(newSymbol));
        }

        public void Add(CustomSymbol newSymbol)
        {
            lock (SyncObj)
            {
                CheckState();

                if (_symbols.ContainsKey(newSymbol.Name))
                    throw new ArgumentException("Symbol " + newSymbol.Name + " already exists!");

                var id = Guid.NewGuid();
                _metadataStorage.Write(id, newSymbol);
                _symbols.Add(newSymbol.Name, new CustomSymbolModel(id, newSymbol));
            };
        }

        public Task RemoveAsync(string symbol)
        {
            return Task.Factory.StartNew(() => Remove(symbol));
        }

        public void Remove(string symbol)
        {
            lock (SyncObj)
            {
                CheckState();

                CustomSymbolModel smbEntity;
                if (_symbols.TryGetValue(symbol, out smbEntity))
                {
                    // clear cache 
                    foreach (var seriesKey in Keys.Snapshot)
                    {
                        if (seriesKey.Symbol == symbol)
                            RemoveSeries(seriesKey);
                    }

                    // remove symbol
                    _metadataStorage.Remove(smbEntity.StorageId);
                    _symbols.Remove(symbol);
                }
            }
        }
    }
}
