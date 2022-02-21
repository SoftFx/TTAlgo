using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage
{
    internal sealed class SymbolStorageSeries : IStorageSeries
    {
        private readonly FeedStorageBase.FeedHandler _storage;
        private readonly FeedCacheKey _key;


        public ISeriesKey Key => _key;

        public double? Size { get; private set; }


        public SymbolStorageSeries(FeedCacheKey key, FeedStorageBase.FeedHandler storage)
        {
            _key = key;
            _storage = storage;
        }

        public Task<bool> TryRemove()
        {
            return _storage.RemoveSeries(_key);
        }

        public ActorChannel<Slice<DateTime, BarData>> IterateBarCache(DateTime from, DateTime to)
        {
            return _storage.IterateBarCacheAsync(_key, from, to);
        }

        public ActorChannel<Slice<DateTime, QuoteInfo>> IterateTickCache(DateTime from, DateTime to)
        {
            return _storage.IterateTickCacheAsync(_key, from, to);
        }

        internal async Task<IStorageSeries> LoadSize()
        {
            Size = await _storage.GetCollectionSize(_key);

            return this;
        }
    }
}