using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib.Math;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage
{
    internal sealed class SymbolStorageSeries : IStorageSeries
    {
        private readonly FeedStorageBase.FeedHandler _storage;
        private readonly FeedCacheKey _key;

        private double _size;


        public ISeriesKey Key => _key;

        public double Size
        {
            get => _size;

            set
            {
                if (_size.E(value))
                    return;

                _size = value;

                SeriesUpdated?.Invoke(value);
            }
        }


        public event Action<double> SeriesUpdated;


        public SymbolStorageSeries(FeedCacheKey key, FeedStorageBase.FeedHandler storage, double size)
        {
            _key = key;
            _size = size;
            _storage = storage;
        }


        public Task<bool> TryRemove() => _storage.RemoveSeries(_key);


        public ActorChannel<Slice<DateTime, BarData>> IterateBarCache(DateTime from, DateTime to)
        {
            return _storage.IterateBarCacheAsync(_key, from, to);
        }

        public ActorChannel<Slice<DateTime, QuoteInfo>> IterateTickCache(DateTime from, DateTime to)
        {
            return _storage.IterateTickCacheAsync(_key, from, to);
        }
    }
}