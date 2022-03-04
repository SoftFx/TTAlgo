using ActorSharp;
using Machinarium.Qnil;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.FeedStorage.Api;


namespace TickTrader.FeedStorage
{
    internal sealed class SymbolCatalog : ISymbolCatalog
    {
        private readonly VarDictionary<ISymbolKey, ISymbolData> _allSymbols;
        private readonly CustomFeedStorage.Handler _customStorage;
        private readonly OnlineFeedStorage.Handler _onlineStorage;
        private readonly IClientFeedProvider _client;


        public ISymbolCollection OnlineCollection => _onlineStorage;

        public ISymbolCollection CustomCollection => _customStorage;

        public IReadOnlyList<ISymbolData> AllSymbols => _allSymbols.OrderBy((k, v) => k).Snapshot;

        public ISymbolData this[ISymbolKey key] => _allSymbols.TryGetValue(key, out var value) ? value : null;



        public SymbolCatalog(IClientFeedProvider client)
        {
            _client = client;

            _allSymbols = new VarDictionary<ISymbolKey, ISymbolData>();
            _customStorage = new CustomFeedStorage.Handler(Actor.SpawnLocal<CustomFeedStorage>());
            _onlineStorage = new OnlineFeedStorage.Handler(Actor.SpawnLocal<OnlineFeedStorage>());
        }


        public async Task<ISymbolCatalog> ConnectClient(IOnlineStorageSettings settings)
        {
            await _onlineStorage.Start(_client, settings);

            SubscribeToCollection(OnlineCollection);

            return this;
        }

        public Task DisconnectClient()
        {
            UnsubscribeCollection(OnlineCollection);

            return _onlineStorage.Stop();
        }


        public async Task<ISymbolCatalog> OpenCustomStorage(ICustomStorageSettings settings)
        {
            if (!_customStorage.IsStarted)
            {
                await _customStorage.Start(settings.FolderPath);

                SubscribeToCollection(CustomCollection);
            }

            return this;
        }

        public Task CloseCustomStorage()
        {
            UnsubscribeCollection(CustomCollection);

            return _customStorage.Stop();
        }

        public Task CloseCatalog()
        {
            return DisconnectClient().ContinueWith(_ => CloseCustomStorage()).Unwrap();
        }


        private void SubscribeToCollection(ISymbolCollection collection)
        {
            collection.Symbols.ForEach(SymbolAddedHandler);

            collection.SymbolAdded += SymbolAddedHandler;
            collection.SymbolRemoved += SymbolRemovedHandler;
            collection.SymbolUpdated += SymbolUpdateHandler;
        }

        private void UnsubscribeCollection(ISymbolCollection collection)
        {
            if (collection == null)
                return;

            collection.Symbols.ForEach(SymbolRemovedHandler);

            collection.SymbolAdded -= SymbolAddedHandler;
            collection.SymbolRemoved -= SymbolRemovedHandler;
            collection.SymbolUpdated -= SymbolUpdateHandler;
        }

        private void SymbolAddedHandler(ISymbolData symbol) => _allSymbols.Add(symbol.Key, symbol);

        private void SymbolRemovedHandler(ISymbolData symbol) => _allSymbols.Remove(symbol);

        private void SymbolUpdateHandler(ISymbolData oldSymbol, ISymbolData newSymbol) => _allSymbols[oldSymbol] = newSymbol;
    }
}
