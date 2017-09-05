using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Core;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.LevelDb;

namespace TickTrader.Algo.Common.Model
{
    public class FeedCache
    {
        private object _sync = new object();
        private Dictionary<FeedCacheKey, SeriesStorage<DateTime, BarEntity>> _barCollections = new Dictionary<FeedCacheKey, SeriesStorage<DateTime, BarEntity>>();
        private LevelDbStorage _diskStorage;
        private string _dbFolder;

        public FeedCache(string dbFolder)
        {
            _dbFolder = dbFolder;
        }

        public IEnumerable<FeedCacheKey> Keys => _barCollections.Keys;
        public object SyncObj => _sync;
        public event Action<FeedCacheKey> Added;
        public event Action<FeedCacheKey> Removed;

        public void Start()
        {
            lock (_sync)
            {
                var loadedKeys = new List<FeedCacheKey>();
                _diskStorage = new LevelDbStorage(_dbFolder);
                foreach (var collectionName in _diskStorage.Collections)
                {
                    var key = FeedCacheKey.Deserialize(collectionName);
                    loadedKeys.Add(key);
                }

                foreach (var key in loadedKeys)
                {
                    var collection = CreateBarCollection(key);
                    _barCollections.Add(key, collection);
                }
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                _barCollections.Clear();
                _diskStorage.Dispose();
                _diskStorage = null;
            }
        }

        public double? GetCollectionSize(FeedCacheKey key)
        {
            return GetBarCollection(key)?.GetSize();
        }

        public KeyRange<DateTime> GetFirstBarRange(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to)
        {
            var key = new FeedCacheKey(symbol, frame, priceType);
            return GetBarCollection(key)?.GetFirstRange(from, to);
        }

        public IEnumerable<KeyRange<DateTime>> IterateCacheKeys(FeedCacheKey key)
        {
            return IterateCacheKeys(key, DateTime.MinValue, DateTime.MaxValue);
        }

        public IEnumerable<KeyRange<DateTime>> IterateCacheKeys(FeedCacheKey key, DateTime from, DateTime to)
        {
            return GetBarCollection(key)?.IterateRanges(from, to);
        }

        public IEnumerable<Slice<DateTime, BarEntity>> IterateBarCache(FeedCacheKey key, DateTime from, DateTime to)
        {
            return GetBarCollection(key)?.IterateSlices(from, to) ?? Enumerable.Empty<Slice<DateTime, BarEntity>>();
        }

        public Slice<DateTime, BarEntity> QueryBarCache(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to)
        {
            var key = new FeedCacheKey(symbol, frame, priceType);
            return GetBarCollection(key)?.GetFirstSlice(from, to);
        }

        public void Put(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to, BarEntity[] values)
        {
            lock (_sync)
            {
                var key = new FeedCacheKey(symbol, frame, priceType);
                var collection = GetBarCollection(key, true);
                collection.Write(from, to, values);
            }
        }

        private SeriesStorage<DateTime, BarEntity> GetBarCollection(FeedCacheKey key, bool addIfMissing = false)
        {
            lock (_sync)
            {
                SeriesStorage<DateTime, BarEntity> series;
                _barCollections.TryGetValue(key, out series);

                if (series == null && addIfMissing)
                {
                    series = CreateBarCollection(key);
                    _barCollections.Add(key, series);
                    Added?.Invoke(key);
                }

                return series;
            }
        }

        private SeriesStorage<DateTime, BarEntity> CreateBarCollection(FeedCacheKey key)
        {
            return SeriesStorage.SeriesStorage.Create(_diskStorage, new DateTimeKeySerializer(), new BarSerializer(key.Frame), b => b.OpenTime, key.Serialize());
        }
    }
}
