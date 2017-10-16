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
        private DynamicDictionary<FeedCacheKey, ISeriesStorage<DateTime>> _series = new DynamicDictionary<FeedCacheKey, ISeriesStorage<DateTime>>();
        private LevelDbStorage _diskStorage;

        public FeedCache()
        {
        }

        public IDynamicSetSource<FeedCacheKey> Keys => _series.Keys;
        public LevelDbStorage Database => _diskStorage;
        protected object SyncObj => _syncObj;

        public virtual void Start(string folder)
        {
            lock (_syncObj)
            {
                _series.Clear();

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
                CheckState();

                return GetSeries(key)?.GetSize();
            }
        }

        #region Bar History

        public KeyRange<DateTime> GetFirstRange(string symbol, Api.TimeFrames frame, Api.BarPriceType? priceType, DateTime from, DateTime to)
        {
            lock (_syncObj)
            {
                var key = new FeedCacheKey(symbol, frame, priceType);
                return GetSeries(key)?.GetFirstRange(from, to);
            }
        }

        public KeyRange<DateTime> GetLastRange(string symbol, Api.TimeFrames frame, Api.BarPriceType? priceType, DateTime from, DateTime to)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, frame, priceType);
                return GetSeries(key)?.GetFirstRange(from, to);
            }
        }

        public Slice<DateTime, BarEntity> GetFirstBarSlice(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, frame, priceType);
                return GetSeries<BarEntity>(key)?.GetFirstSlice(from, to);
            }
        }

        private IEnumerable<KeyRange<DateTime>> IterateCacheKeys(FeedCacheKey key)
        {
            return IterateCacheKeys(key, DateTime.MinValue, DateTime.MaxValue);
        }

        private IEnumerable<KeyRange<DateTime>> IterateCacheKeys(FeedCacheKey key, DateTime from, DateTime to)
        {
            return GetSeries(key)?.IterateRanges(from, to) ?? Enumerable.Empty<KeyRange<DateTime>>();
        }

        public IEnumerable<Slice<DateTime, BarEntity>> IterateBarCache(FeedCacheKey key, DateTime from, DateTime to)
        {
            return IterateBarCacheInternal(key, from, to).GetSyncWrapper(_syncObj);
        }

        private IEnumerable<Slice<DateTime, BarEntity>> IterateBarCacheInternal(FeedCacheKey key, DateTime from, DateTime to)
        {
            CheckState();
            foreach (var entry in GetSeries<BarEntity>(key)?.IterateSlices(from, to))
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
                return GetSeries<BarEntity>(key)?.GetFirstSlice(from, to);
            }
        }

        public void Put(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to, BarEntity[] values)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, frame, priceType);
                var collection = GetSeries<BarEntity>(key, true);
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
            foreach (var entry in GetSeries<QuoteEntity>(key)?.IterateSlices(from, to))
            {
                CheckState();
                yield return entry;
            }
        }

        public void Put(string symbol, Api.TimeFrames timeFrame, DateTime from, DateTime to, QuoteEntity[] values)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, timeFrame);
                var collection = GetSeries<QuoteEntity>(key, true);
                collection.Write(from, to, values);
            }
        }

        public void Put(string symbol, Api.TimeFrames timeFrame, Slice<DateTime, QuoteEntity> slice)
        {
            lock (_syncObj)
            {
                CheckState();

                var key = new FeedCacheKey(symbol, timeFrame);
                var collection = GetSeries<QuoteEntity>(key, true);
                collection.Write(slice);
            }
        }

        #endregion

        private ISeriesStorage<DateTime> CreateCollection(FeedCacheKey key)
        {
            ISeriesStorage<DateTime> collection;

            if (key.Frame == Api.TimeFrames.Ticks || key.Frame == Api.TimeFrames.TicksLevel2)
                collection = SeriesStorage.SeriesStorage.Create(_diskStorage, new DateTimeKeySerializer(), new TickSerializer(key.Symbol), b => b.Time, key.Serialize());
            else
                collection = SeriesStorage.SeriesStorage.Create(_diskStorage, new DateTimeKeySerializer(), new BarSerializer(key.Frame), b => b.OpenTime, key.Serialize());

            _series.Add(key, collection);
            return collection;
        }

        public void RemoveSeries(FeedCacheKey seriesKey)
        {
            lock (_syncObj)
            {
                CheckState();

                ISeriesStorage<DateTime> series;
                _series.TryGetValue(seriesKey, out series);

                if (series != null)
                {
                    series.Drop();
                    _series.Remove(seriesKey);
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

        private ISeriesStorage<DateTime> GetSeries(FeedCacheKey key, bool addIfMissing = false)
        {
            ISeriesStorage<DateTime> series;
            _series.TryGetValue(key, out series);

            if (addIfMissing && series == null)
            {
                CreateCollection(key);
                _series.TryGetValue(key, out series);
            }

            return series;
        }

        private SeriesStorage<DateTime, TVal> GetSeries<TVal>(FeedCacheKey key, bool addIfMissing = false)
        {
            return (SeriesStorage<DateTime, TVal>)GetSeries(key, addIfMissing);
        }
    }
}
