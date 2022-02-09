using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage
{
    public class SymbolStorageSeries
    {
        private FeedStorageBase.FeedHandler _storage;

        public SymbolStorageSeries(FeedCacheKey key, ISymbolData symbolModel, FeedStorageBase.FeedHandler storage)
        {
            Key = key;
            Symbol = symbolModel;
            _storage = storage;
        }

        public FeedCacheKey Key { get; }
        public ISymbolData Symbol { get; }

        public Task<double?> GetCollectionSize()
        {
            return _storage.GetCollectionSize(Key);
        }

        public Task Remove()
        {
            return _storage.RemoveSeries(Key);
        }

        public ActorChannel<Slice<DateTime, BarData>> IterateBarCache(DateTime from, DateTime to)
        {
            return _storage.IterateBarCacheAsync(Key, from, to);
        }

        public ActorChannel<Slice<DateTime, QuoteInfo>> IterateTickCache(DateTime from, DateTime to)
        {
            return _storage.IterateTickCacheAsync(Key, from, to);
        }
    }
}
