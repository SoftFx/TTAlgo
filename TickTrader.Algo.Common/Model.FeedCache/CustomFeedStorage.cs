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
        private DynamicDictionary<string, CustomSymbol> _symbols = new DynamicDictionary<string, CustomSymbol>();

        public override void Start(string dbFolder)
        {
            lock (SyncObj)
            {
                _symbols.Clear();

                base.Start(dbFolder);

                _metadataStorage = Database.GetCollection("symbols", new GuidKeySerializer(), new ProtoValueSerializer<CustomSymbol>());

                foreach (var entry in _metadataStorage.Iterate(Guid.Empty))
                {
                    var smb = entry.Value;
                    smb.StorageId = entry.Key;

                    _symbols.Add(smb.Name, smb);
                }
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

        public IDynamicDictionarySource<string, CustomSymbol> GetSymbolsSyncCopy(ISyncContext context)
        {
            lock (SyncObj) return new DictionarySyncrhonizer<string, CustomSymbol>(_symbols, context);
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

                newSymbol.StorageId = Guid.NewGuid();
                _metadataStorage.Write(newSymbol.StorageId, newSymbol);
                _symbols.Add(newSymbol.Name, newSymbol);
            };
        }

        public void Update(CustomSymbol symbolCfg)
        {
            lock (SyncObj)
            {
                CheckState();

                var oldEntity = _symbols[symbolCfg.Name];

                if (oldEntity.Name != symbolCfg.Name)
                    throw new ArgumentException("Changing name is not supported!");

                symbolCfg.StorageId = oldEntity.StorageId;
                _metadataStorage.Write(oldEntity.StorageId, symbolCfg);
                _symbols[symbolCfg.Name] = symbolCfg;
            }
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

                CustomSymbol smbEntity;
                if (_symbols.TryGetValue(symbol, out smbEntity))
                {
                    var toRemove = Keys.Snapshot.Where(k => k.Symbol == symbol).ToList();

                    // clear cache 
                    toRemove.ForEach(RemoveSeries);

                    // remove symbol
                    _metadataStorage.Remove(smbEntity.StorageId);
                    _symbols.Remove(symbol);
                }
            }
        }
    }
}
