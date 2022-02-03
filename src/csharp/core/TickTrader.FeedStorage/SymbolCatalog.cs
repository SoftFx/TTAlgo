using ActorSharp;
using Machinarium.Qnil;
using System;
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
        private readonly CustomFeedStorage.Handler _customStorage;
        private readonly FeedCache.Handler _onlineSymbolsStorag;
        private readonly FeedProvider _feedProvider;
        private FeedProvider.Handler _feedHandler;
        private readonly IClientFeedProvider _client;
        private readonly ICustomStorageSettings _customStorageSettings;


        public ISymbolCollection<SymbolData> OnlineCollection => throw new NotImplementedException();

        public ISymbolCollection<CustomSymbol> CustomCollection => throw new NotImplementedException();

        public SymbolData this[string name] => throw new NotImplementedException();

        List<SymbolData> ISymbolCatalog.AllSymbols => throw new NotImplementedException();


        public SymbolCatalog(IClientFeedProvider client, ICustomStorageSettings settings)
        {
            _client = client;
            _customStorageSettings = settings;

            _feedProvider = new FeedProvider();

            _customStorage = new CustomFeedStorage.Handler(Actor.SpawnLocal<CustomFeedStorage>());


            CustomSymbols = _customStorage.Symbols.Select<SymbolKey, CustomSymbol, SymbolData>(
                (k, s) => new CustomSymbolData(s, _customStorage));

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

        public async Task<ISymbolCatalog> Connect(IOnlineStorageSettings settings)
        {
            await StartCustomStorage(_customStorageSettings);
            await _customStorage.SyncData();
            // if storage don't run

            await _feedProvider.Start(_client, settings);
            _feedHandler = new FeedProvider.Handler();

            //OnlineSymbols = _client.Symbols.Select(s => CreateSymbolData(s, _feedHandler));

            return this;
        }

        private Task StartCustomStorage(ICustomStorageSettings settings)
        {
            return _customStorage.Start(settings.FolderPath);
        }

        public IVarSet<SymbolKey, SymbolData> AllSymbols { get; }
        public IVarSet<SymbolKey, SymbolData> OnlineSymbols { get; private set; }
        public IVarSet<SymbolKey, SymbolData> CustomSymbols { get; }

        public IObservableList<SymbolData> ObservableSymbols { get; }
        public IObservableList<SymbolData> ObservableOnlineSymbols { get; }
        public IObservableList<SymbolData> ObservableCustomSymbols { get; }


        public Task AddCustomSymbol(CustomSymbol customSymbol)
        {
            return _customStorage.Add(customSymbol);
        }

        public Task Update(CustomSymbol customSymbol)
        {
            return _customStorage.Update(customSymbol);
        }

        public Task<ActorChannel<SliceInfo>> DownloadBarSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        {
            return _feedHandler?.DownloadBarSeriesToStorage(symbol, timeframe, marketSide, from, to);
        }

        public Task<ActorChannel<SliceInfo>> DownloadTickSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, DateTime from, DateTime to)
        {
            return _feedHandler?.DownloadTickSeriesToStorage(symbol, timeframe, from, to);
        }

        public SymbolData GetSymbol(ISetupSymbolInfo info)
        {
            var key = new SymbolKey(info.Name, info.Origin);

            if (info.Origin == SymbolConfig.Types.SymbolOrigin.Online)
                return OnlineSymbols.Snapshot[key];
            else if (info.Origin == SymbolConfig.Types.SymbolOrigin.Custom)
                return CustomSymbols.Snapshot[key];

            throw new Exception("Unsupported symbol origin: " + info.Origin);
        }

        public async Task Close()
        {
            await _customStorage.Stop();
        }

        private KeyValuePair<SymbolKey, SymbolData> CreateSymbolData(SymbolInfo smb, FeedProvider.Handler provider)
        {
            var key = new SymbolKey(smb.Name, SymbolConfig.Types.SymbolOrigin.Online);
            var data = new OnlineSymbolData(smb, provider);
            return new KeyValuePair<SymbolKey, SymbolData>(key, data);
        }
    }
}
