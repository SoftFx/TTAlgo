using ActorSharp;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.LevelDb;
using TickTrader.SeriesStorage.Protobuf;

namespace TickTrader.Algo.Common.Model
{
    public class CustomFeedStorage : FeedCache
    {
        private ICollectionStorage<Guid, CustomSymbol> _metadataStorage;
        private VarDictionary<string, CustomSymbol> _symbols = new VarDictionary<string, CustomSymbol>();

        public new class Handler : FeedCache.Handler
        {
            private Ref<CustomFeedStorage> _ref;
            private VarDictionary<string, CustomSymbol> _symbols = new VarDictionary<string, CustomSymbol>();

            public Handler(Ref<CustomFeedStorage> actorRef) : base(actorRef.Cast<CustomFeedStorage, FeedCache>())
            {
                _ref = actorRef;
            }

            public IVarSet<string, CustomSymbol> Symbols => _symbols;

            public Task Add(CustomSymbol newSymbol) => _ref.Call(a => a.Add(newSymbol));
            public Task Remove(string symbol) => _ref.Call(a => a.Remove(symbol));
            public Task Update(CustomSymbol symbolCfg) => _ref.Call(a => a.Update(symbolCfg));
        }

        protected override void Start(string dbFolder)
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

        protected override bool IsSpecialCollection(string name)
        {
            return name == "symbols";
        }

        protected override void Stop()
        {
            _metadataStorage.Dispose();
            _metadataStorage = null;

            base.Stop();
        }

        //public IVarSet<string, CustomSymbol> GetSymbolsSyncCopy(ISyncContext context)
        //{
        //    lock (SyncObj) return new DictionarySyncrhonizer<string, CustomSymbol>(_symbols, context);
        //}

        //public Task AddAsync(CustomSymbol newSymbol)
        //{
        //    return Task.Factory.StartNew(() => Add(newSymbol));
        //}

        private void Add(CustomSymbol newSymbol)
        {
            CheckState();

            if (_symbols.ContainsKey(newSymbol.Name))
                throw new ArgumentException("Symbol " + newSymbol.Name + " already exists!");

            newSymbol.StorageId = Guid.NewGuid();
            _metadataStorage.Write(newSymbol.StorageId, newSymbol);
            _symbols.Add(newSymbol.Name, newSymbol);
        }

        private void Update(CustomSymbol symbolCfg)
        {
            CheckState();

            var oldEntity = _symbols[symbolCfg.Name];

            if (oldEntity.Name != symbolCfg.Name)
                throw new ArgumentException("Changing name is not supported!");

            symbolCfg.StorageId = oldEntity.StorageId;
            _metadataStorage.Write(oldEntity.StorageId, symbolCfg);
            _symbols[symbolCfg.Name] = symbolCfg;
        }

        private Task RemoveAsync(string symbol)
        {
            return Task.Factory.StartNew(() => Remove(symbol));
        }

        private void Remove(string symbol)
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
