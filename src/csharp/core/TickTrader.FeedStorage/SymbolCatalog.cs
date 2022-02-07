using ActorSharp;
using Machinarium.Qnil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    internal sealed class SymbolCatalog : ISymbolCatalog
    {
        private readonly VarDictionary<ISymbolKey, ISymbolData> _allSymbols;
        private readonly CustomFeedStorage.Handler _customStorage;

        //private readonly FeedProvider _feedProvider;
        private FeedProvider.Handler _feedHandler;
        private readonly IClientFeedProvider _client;


        public ISymbolCollection<ISymbolData> OnlineCollection => _feedHandler?.Cache.Collection;

        public ISymbolCollection<ISymbolData> CustomCollection => _customStorage?.Collection;

        public ISymbolData this[ISymbolKey key] => _allSymbols.TryGetValue(key, out var value) ? value : null;

        IReadOnlyList<ISymbolData> ISymbolCatalog.AllSymbols => _allSymbols.OrderBy((k, v) => k).Snapshot;


        public SymbolCatalog(IClientFeedProvider client)
        {
            _client = client;

            //_feedProvider = new FeedProvider();
            _allSymbols = new VarDictionary<ISymbolKey, ISymbolData>();
            _customStorage = new CustomFeedStorage.Handler(Actor.SpawnLocal<CustomFeedStorage>());


            //CustomSymbols = _customStorage.Symbols.Select<SymbolKey, CustomSymbol, SymbolData>(
            //    (k, s) => new CustomSymbolData(s, _customStorage));

            //var sortedOnlineList = OnlineSymbols.OrderBy((k, v) => v.Name);
            //var sortedCustomList = CustomSymbols.OrderBy((k, v) => v.Name);

            //ObservableOnlineSymbols = sortedOnlineList.AsObservable();
            //ObservableCustomSymbols = sortedCustomList.AsObservable();

            //AllSymbols = VarCollection.Combine(OnlineSymbols, CustomSymbols);

            //var sortedList = AllSymbols.OrderBy((k, v) => k, new SymbolKeyComparer());

            //ObservableSymbols = sortedList.AsObservable();

            //AllSymbols.Updated += a =>
            //{
            //    if (a.OldItem != null)
            //        a.OldItem.OnRemoved();
            //};
        }

        private void SymbolAddedHandler(ISymbolData symbol)
        {
            //var token = symbol.ToSymbolToken();
            //var key = new SymbolKey(token.Name, token.Origin);

            _allSymbols.Add(symbol.StorageKey, symbol);
        }

        public async Task<ISymbolCatalog> ConnectClient(IOnlineStorageSettings settings)
        {
            //await _customStorage.SyncData();
            // if storage don't run

            //await _feedProvider.Start(_client, settings);

            _feedHandler = new FeedProvider.Handler(_client, settings);

            await _feedHandler.Init();

            OnlineCollection.Symbols.ForEach(SymbolAddedHandler);
            OnlineCollection.SymbolAdded += SymbolAddedHandler;
            //OnlineSymbols = _client.Symbols.Select(s => CreateSymbolData(s, _feedHandler));

            return this;
        }

        public Task DisconnectClient() => _feedHandler.Stop();

        public async Task<ISymbolCatalog> OpenCustomStorage(ICustomStorageSettings settings)
        {
            if (!_customStorage.IsStarted)
            {
                await _customStorage.Start(settings.FolderPath);

                CustomCollection.Symbols.ForEach(SymbolAddedHandler);
                CustomCollection.SymbolAdded += SymbolAddedHandler;
            }

            return this;
        }


        //private void InitNewCollectionToAllSymbols(List<ISymbolData> symbols)
        //{
        //    symbols.ForEach(SymbolAddedHandler);
        //}

        //private Task StartCustomStorage(ICustomStorageSettings settings)
        //{
        //    return 
        //}

        //public IVarSet<SymbolKey, SymbolData> AllSymbols { get; }
        //public IVarSet<SymbolKey, SymbolData> OnlineSymbols { get; private set; }
        //public IVarSet<SymbolKey, SymbolData> CustomSymbols { get; }

        //public IObservableList<SymbolData> ObservableSymbols { get; }
        //public IObservableList<SymbolData> ObservableOnlineSymbols { get; }
        //public IObservableList<SymbolData> ObservableCustomSymbols { get; }


        //public Task AddCustomSymbol(CustomSymbol customSymbol)
        //{
        //    return _customStorage.Add(customSymbol);
        //}

        //public Task Update(CustomSymbol customSymbol)
        //{
        //    return _customStorage.Update(customSymbol);
        //}

        public Task<ActorChannel<SliceInfo>> DownloadBarSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        {
            return _feedHandler?.DownloadBarSeriesToStorage(symbol, timeframe, marketSide, from, to);
        }

        public Task<ActorChannel<SliceInfo>> DownloadTickSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, DateTime from, DateTime to)
        {
            return _feedHandler?.DownloadTickSeriesToStorage(symbol, timeframe, from, to);
        }

        //public SymbolData GetSymbol(ISetupSymbolInfo info)
        //{
        //    var key = new SymbolKey(info.Name, info.Origin);

        //    //if (info.Origin == SymbolConfig.Types.SymbolOrigin.Online)
        //    //    return OnlineSymbols.Snapshot[key];
        //    //else if (info.Origin == SymbolConfig.Types.SymbolOrigin.Custom)
        //    //    return CustomSymbols.Snapshot[key];

        //    throw new Exception("Unsupported symbol origin: " + info.Origin);
        //}

        public Task CloseCustomStorage() =>_customStorage.Stop();

        public Task CloseCatalog()
        {
            return DisconnectClient().ContinueWith(_ => CloseCustomStorage()).Unwrap();
        }

        //private KeyValuePair<SymbolKey, SymbolData> CreateSymbolData(SymbolInfo smb, FeedProvider.Handler provider)
        //{
        //    var key = new SymbolKey(smb.Name, SymbolConfig.Types.SymbolOrigin.Online);
        //    var data = new OnlineSymbol(smb, provider);
        //    return new KeyValuePair<SymbolKey, SymbolData>(key, data);
        //}
    }
}
