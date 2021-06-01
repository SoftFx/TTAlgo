using ActorSharp;
using ActorSharp.Lib;
using Machinarium.Qnil;
using System;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.Protobuf;

namespace TickTrader.Algo.Account.FeedStorage
{
    public class CustomFeedStorage : FeedCache
    {
        private ICollectionStorage<Guid, CustomSymbol> _metadataStorage;
        private VarDictionary<string, CustomSymbol> _symbols = new VarDictionary<string, CustomSymbol>();
        private ActorEvent<EntityUpdateArgs<CustomSymbol>> _symbolChangeListeners = new ActorEvent<EntityUpdateArgs<CustomSymbol>>();

        public CustomFeedStorage()
        {
            _symbols.Updated += a =>
            {
                if (a.Action == DLinqAction.Insert)
                    _symbolChangeListeners.FireAndForget(new EntityUpdateArgs<CustomSymbol>(a.NewItem, EntityUpdateActions.Add));
                else if(a.Action == DLinqAction.Remove)
                    _symbolChangeListeners.FireAndForget(new EntityUpdateArgs<CustomSymbol>(a.OldItem, EntityUpdateActions.Remove));
                else if(a.Action == DLinqAction.Replace)
                    _symbolChangeListeners.FireAndForget(new EntityUpdateArgs<CustomSymbol>(a.NewItem, EntityUpdateActions.Replace));
            };
        }

        public new class Handler : FeedCache.Handler
        {
            private Ref<CustomFeedStorage> _ref;
            private VarDictionary<SymbolKey, CustomSymbol> _symbols = new VarDictionary<SymbolKey, CustomSymbol>();
            private ActorCallback<EntityUpdateArgs<CustomSymbol>> _smbChangedCallback;

            public Handler(Ref<CustomFeedStorage> actorRef) : base(actorRef.Cast<CustomFeedStorage, FeedCache>())
            {
                _ref = actorRef;
            }

            public async override Task SyncData()
            {
                _symbols.Clear();

                await base.SyncData();

                _smbChangedCallback = ActorCallback.Create<EntityUpdateArgs<CustomSymbol>>(u =>
                {
                    if (u.Action == EntityUpdateActions.Add)
                        _symbols.Add(GetKey(u.Entity), u.Entity);
                    else if (u.Action == EntityUpdateActions.Remove)
                        _symbols.Remove(GetKey(u.Entity));
                    else if (u.Action == EntityUpdateActions.Replace)
                        _symbols[GetKey(u.Entity)] = u.Entity;
                });

                var snapshot = await _ref.Call(a =>
                {
                    a._symbolChangeListeners.Add(_smbChangedCallback);
                    return a._symbols.Values.ToList();
                });

                foreach (var smb in snapshot)
                    _symbols.Add(GetKey(smb), smb);
            }

            public override void Dispose()
            {
                if (_smbChangedCallback != null)
                {
                    _symbols.Clear();
                    _ref.Send(a => a._symbolChangeListeners.Remove(_smbChangedCallback));
                    _smbChangedCallback = null;
                }

                base.Dispose();
            }

            public IVarSet<SymbolKey, CustomSymbol> Symbols => _symbols;

            public Task Add(CustomSymbol newSymbol) => _ref.Call(a => a.Add(newSymbol));
            public Task Remove(string symbol) => _ref.Call(a => a.Remove(symbol));
            public Task Update(CustomSymbol symbolCfg) => _ref.Call(a => a.Update(symbolCfg));

            private SymbolKey GetKey(CustomSymbol smb)
            {
                return new SymbolKey(smb.Name, SymbolConfig.Types.SymbolOrigin.Custom);
            }
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
