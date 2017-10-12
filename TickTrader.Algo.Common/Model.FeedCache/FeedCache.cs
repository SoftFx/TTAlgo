using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.LevelDb;

namespace TickTrader.Algo.Common.Model
{
    public class FeedCache
    {
        private object _syncObj = new object();
        private DynamicDictionary<FeedCacheKey, SeriesStorage<DateTime, BarEntity>> _barCollections = new DynamicDictionary<FeedCacheKey, SeriesStorage<DateTime, BarEntity>>();
        private DynamicDictionary<FeedCacheKey, SeriesStorage<DateTime, QuoteEntity>> _tickCollections = new DynamicDictionary<FeedCacheKey, SeriesStorage<DateTime, QuoteEntity>>();
        private LevelDbStorage _diskStorage;

        public FeedCache()
        {
            Keys = Dynamic.Union(_barCollections.Keys, _tickCollections.Keys);
        }

        public IDynamicSetSource<FeedCacheKey> Keys { get; }
        public LevelDbStorage Database => _diskStorage;
        protected object SyncObj => _syncObj;

        public virtual void Start(string folder)
        {
            lock (_syncObj)
            {
                _barCollections.Clear();

                if (_diskStorage != null)
                    throw new InvalidOperationException("Already started!");

                _diskStorage = new LevelDbStorage(folder);

                var loadedKeys = new List<FeedCacheKey>();

                foreach (var collectionName in _diskStorage.Collections)
                {
                    if (!IsSpecialCollection(collectionName))
                    {
                        var key = FeedCacheKey.Deserialize(collectionName);
                        loadedKeys.Add(key);
                    }
                }

                foreach (var key in loadedKeys)
                    CreateCollection(key);
            }
        }

        protected virtual bool IsSpecialCollection(string name)
        {
            return false;
        }

        public virtual void Stop()
        {
            lock (_syncObj)
            {
                if (_diskStorage != null)
                {
                    _diskStorage.Dispose();
                    _diskStorage = null;
                }
            }
        }

        public Tuple<DateTime, DateTime> GetRange(FeedCacheKey key, bool custom)
        {
            lock (_syncObj)
            {
                CheckState();

                bool hasValues = false;
                var min = DateTime.MinValue;
                var max = DateTime.MaxValue;

                foreach (var r in IterateCacheKeys(key))
                {
                    if (!hasValues)
                    {
                        min = r.From;
                        hasValues = true;
                    }

                    max = r.To;
                }

                return hasValues ? new Tuple<DateTime, DateTime>(min, max) : null;
            }
        }

        public Task<Tuple<DateTime, DateTime>> GetRangeAsync(FeedCacheKey key, bool custom)
        {
            return Task.Factory.StartNew(() => GetRange(key, custom));
        }

        public double? GetCollectionSize(FeedCacheKey key)
        {
            lock (_syncObj)
            {
                if (key.Frame == Api.TimeFrames.Ticks)
                    return GetTicksCollection(key)?.GetSize();
                else
                    return GetBarCollection(key)?.GetSize();
            }
        }

        #region Bar History

        public KeyRange<DateTime> GetFirstBarRange(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to)
        {
            lock (_syncObj)
            {
                var key = new FeedCacheKey(symbol, frame, priceType);
                return GetBarCollection(key)?.GetFirstRange(from, to);
            }
        }

        public KeyRange<DateTime> GetLastBarRange(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, frame, priceType);
                return GetBarCollection(key)?.GetFirstRange(from, to);
            }
        }

        public Slice<DateTime, BarEntity> GetFirstBarSlice(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, frame, priceType);
                return GetBarCollection(key)?.GetFirstSlice(from, to);
            }
        }

        private IEnumerable<KeyRange<DateTime>> IterateCacheKeys(FeedCacheKey key)
        {
            return IterateCacheKeys(key, DateTime.MinValue, DateTime.MaxValue);
        }

        private IEnumerable<KeyRange<DateTime>> IterateCacheKeys(FeedCacheKey key, DateTime from, DateTime to)
        {
            if (key.Frame == Api.TimeFrames.Ticks)
                return GetTicksCollection(key)?.IterateRanges(from, to) ?? Enumerable.Empty<KeyRange<DateTime>>();
            else
                return GetBarCollection(key)?.IterateRanges(from, to) ?? Enumerable.Empty<KeyRange<DateTime>>();
        }

        public IEnumerable<Slice<DateTime, BarEntity>> IterateBarCache(FeedCacheKey key, DateTime from, DateTime to)
        {
            return IterateBarCacheInternal(key, from, to).GetSyncWrapper(_syncObj);
        }

        private IEnumerable<Slice<DateTime, BarEntity>> IterateBarCacheInternal(FeedCacheKey key, DateTime from, DateTime to)
        {
            CheckState();
            foreach (var entry in GetBarCollection(key)?.IterateSlices(from, to))
            {
                CheckState();
                yield return entry;
            }
        }

        public Slice<DateTime, BarEntity> QueryBarCache(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, frame, priceType);
                return GetBarCollection(key)?.GetFirstSlice(from, to);
            }
        }

        public void Put(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to, BarEntity[] values)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, frame, priceType);
                var collection = GetBarCollection(key, true);
                collection.Write(from, to, values);
            }
        }

        #endregion

        #region Tick History

        public IEnumerable<Slice<DateTime, QuoteEntity>> IterateTickCache(FeedCacheKey key, DateTime from, DateTime to)
        {
            return IterateTickCacheInternal(key, from, to).GetSyncWrapper(_syncObj);
        }

        private IEnumerable<Slice<DateTime, QuoteEntity>> IterateTickCacheInternal(FeedCacheKey key, DateTime from, DateTime to)
        {
            CheckState();
            foreach (var entry in GetTicksCollection(key)?.IterateSlices(from, to))
            {
                CheckState();
                yield return entry;
            }
        }

        public KeyRange<DateTime> GetLastTickRange(string symbol, bool includeLevel2, DateTime from, DateTime to)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, Api.TimeFrames.Ticks);
                return GetTicksCollection(key)?.GetLastRange(from, to);
            }
        }

        public void Put(string symbol, bool level2, DateTime from, DateTime to, QuoteEntity[] values)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, Api.TimeFrames.Ticks);
                var collection = GetTicksCollection(key, true);
                collection.Write(from, to, values);
            }
        }

        public void Put(string symbol, bool level2, Slice<DateTime, QuoteEntity> slice)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, Api.TimeFrames.Ticks);
                var collection = GetTicksCollection(key, true);
                collection.Write(slice);
            }
        }

        #endregion

        private void CreateCollection(FeedCacheKey key)
        {
            if (key.Frame == Api.TimeFrames.Ticks)
            {
                var collection = SeriesStorage.SeriesStorage.Create(_diskStorage, new DateTimeKeySerializer(), new TickSerializer(key.Symbol), b => b.Time, key.Serialize());
                _tickCollections.Add(key, collection);
            }
            else
            {
                var collection = SeriesStorage.SeriesStorage.Create(_diskStorage, new DateTimeKeySerializer(), new BarSerializer(key.Frame), b => b.OpenTime, key.Serialize());
                _barCollections.Add(key, collection);
            }
        }

        public void RemoveSeries(FeedCacheKey seriesKey)
        {
            lock (_syncObj)
            {
                SeriesStorage<DateTime, BarEntity> series;
                _barCollections.TryGetValue(seriesKey, out series);

                if (series != null)
                {
                    series.Drop();
                    _barCollections.Remove(seriesKey);
                }
            }
        }

        protected void CheckState()
        {
            if (_diskStorage == null)
                throw new Exception("Invalid operation! CustomFeedStorage is not started or already stopped!");
        }

        public IDynamicSetSource<FeedCacheKey> GetKeysSyncCopy(ISyncContext context)
        {
            lock (_syncObj)
                return new SetSynchronizer<FeedCacheKey>(Keys, context);
        }

        private SeriesStorage<DateTime, BarEntity> GetBarCollection(FeedCacheKey key, bool addIfMissing = false)
        {
            if (key.Frame == Api.TimeFrames.Ticks)
                throw new ArgumentException("key");

            SeriesStorage<DateTime, BarEntity> series;
            _barCollections.TryGetValue(key, out series);

            if (series == null && addIfMissing)
            {
                CreateCollection(key);
                _barCollections.TryGetValue(key, out series);
            }

            return series;
        }

        private SeriesStorage<DateTime, QuoteEntity> GetTicksCollection(FeedCacheKey key, bool addIfMissing = false)
        {
            if (key.Frame != Api.TimeFrames.Ticks)
                throw new ArgumentException("key");

            SeriesStorage<DateTime, QuoteEntity> series;
            _tickCollections.TryGetValue(key, out series);

            if (series == null && addIfMissing)
            {
                CreateCollection(key);
                _tickCollections.TryGetValue(key, out series);
            }

            return series;
        }
    }
}
