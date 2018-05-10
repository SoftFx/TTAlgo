using ActorSharp;
using ActorSharp.Lib;
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
    public class FeedCache : Actor
    {
        private VarDictionary<FeedCacheKey, ISeriesStorage<DateTime>> _series = new VarDictionary<FeedCacheKey, ISeriesStorage<DateTime>>();
        private LevelDbStorage _diskStorage;
        private ActorEvent<FeedCacheKey> _addListeners = new ActorEvent<FeedCacheKey>();
        private ActorEvent<FeedCacheKey> _removeListeners = new ActorEvent<FeedCacheKey>();

        protected IVarSet<FeedCacheKey> Keys => _series.Keys;
        protected LevelDbStorage Database => _diskStorage;

        public FeedCache()
        {
            _series.Updated += a =>
            {
                if (a.Action == DLinqAction.Insert)
                    _addListeners.FireAndForget(a.Key);
                else if (a.Action == DLinqAction.Remove)
                    _removeListeners.FireAndForget(a.Key);
            };
        }

        public class Handler
        {
            private VarDictionary<FeedCacheKey, object> _series;
            private Ref<FeedCache> _ref;
            private ActorCallback<FeedCacheKey> addCallback;
            private ActorCallback<FeedCacheKey> removeCallback;

            public Handler(Ref<FeedCache> actorRef)
            {
                _ref = actorRef;
            }

            public virtual async Task SyncData()
            {
                 _series = new VarDictionary<FeedCacheKey, object>();

                addCallback = ActorCallback.Create<FeedCacheKey>(k => _series.Add(k, null));
                removeCallback = ActorCallback.Create<FeedCacheKey>(k => _series.Remove(k));

                var snapshot = await _ref.Call(a =>
                {
                    a._addListeners.Add(addCallback);
                    a._removeListeners.Add(removeCallback);
                    return a._series.Keys.Snapshot.ToList();
                });

                foreach (var key in snapshot)
                    _series.Add(key, null);
            }

            public virtual void Dispose()
            {
                if (_series != null)
                {
                    _series = null;
                    _series.Clear();
                    _ref.Send(a =>
                    {
                        a._addListeners.Remove(addCallback);
                        a._removeListeners.Remove(removeCallback);
                    });
                }
            }

            public IVarSet<FeedCacheKey> Keys => _series?.Keys;

            public Task Start(string folder) => _ref.Call(a => a.Start(folder));
            public Task Stop() => _ref.Call(a => a.Stop());

            public Task Put(FeedCacheKey key, DateTime from, DateTime to, QuoteEntity[] values)
                => Put(key.Symbol, key.Frame, from, to, values);

            public Task Put(string symbol, Api.TimeFrames timeFrame, DateTime from, DateTime to, QuoteEntity[] values)
                => _ref.Call(a => a.Put(symbol, timeFrame, from, to, values));

            public Task Put(string symbol, Api.TimeFrames timeFrame, Slice<DateTime, QuoteEntity> slice)
                => _ref.Call(a => a.Put(symbol, timeFrame, slice));

            public Task Put(FeedCacheKey key, DateTime from, DateTime to, BarEntity[] values)
                => _ref.Call(a => a.Put(key, from, to, values));

            public Task Put(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to, BarEntity[] values)
                => Put(new FeedCacheKey(symbol, frame, priceType), from, to, values);

            public Channel<Slice<DateTime, BarEntity>> IterateBarCacheAsync(FeedCacheKey key, DateTime from, DateTime to)
            {
                var channel = new Channel<Slice<DateTime, BarEntity>>(ChannelDirections.Out, 1);
                _ref.SendChannel(channel, (a, c) => a.IterateBarCache(c, key, from, to));
                return channel;
            }

            public BlockingChannel<Slice<DateTime, BarEntity>> IterateBarCache(FeedCacheKey key, DateTime from, DateTime to)
            {
                return _ref.OpenBlockingChannel<FeedCache, Slice<DateTime, BarEntity>>(ChannelDirections.Out, 2, (a, c) => a.IterateBarCache(c, key, from, to));
            }

            public Channel<KeyRange<DateTime>> IterateCacheKeys(FeedCacheKey key, DateTime from, DateTime to)
            {
                var channel = new Channel<KeyRange<DateTime>> (ChannelDirections.Out, 1);
                _ref.SendChannel(channel, (a, c) => a.IterateCacheKeys(c, key, from, to));
                return channel;
            }

            public Task<KeyRange<DateTime>> GetFirstRange(string symbol, Api.TimeFrames frame, Api.BarPriceType? priceType, DateTime from, DateTime to)
                => _ref.Call(a => a.GetFirstRange(symbol, frame, priceType, from, to));

            public Task<Tuple<DateTime, DateTime>> GetRange(FeedCacheKey key)
                => _ref.Call(a => a.GetRange(key));

            public Task<double?> GetCollectionSize(FeedCacheKey key)
                => _ref.Call(a => a.GetCollectionSize(key));

            public Task RemoveSeries(FeedCacheKey seriesKey)
                => _ref.Call(a => a.RemoveSeries(seriesKey));
        }

        protected virtual void Start(string folder)
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

        protected virtual bool IsSpecialCollection(string name)
        {
            return false;
        }

        protected virtual void Stop()
        {
            if (_diskStorage != null)
            {
                _diskStorage.Dispose();
                _diskStorage = null;
            }
        }

        protected Tuple<DateTime, DateTime> GetRange(FeedCacheKey key)
        {
            CheckState();

            bool hasValues = false;
            var min = DateTime.MinValue;
            var max = DateTime.MaxValue;

            foreach (var r in IterateCacheKeysInternal(key))
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

        //private Task<Tuple<DateTime, DateTime>> GetRangeAsync(FeedCacheKey key, bool custom)
        //{
        //    return Task.Factory.StartNew(() => GetRange(key, custom));
        //}

        protected double? GetCollectionSize(FeedCacheKey key)
        {
            CheckState();

            return GetSeries(key)?.GetSize();
        }

        private void IterateCacheKeys(Channel<KeyRange<DateTime>> channel, FeedCacheKey key, DateTime from, DateTime to)
        {
            channel.WriteAll(() => IterateCacheKeysInternal(key, from, to));
        }

        #region Bar History

        private KeyRange<DateTime> GetFirstRange(string symbol, Api.TimeFrames frame, Api.BarPriceType? priceType, DateTime from, DateTime to)
        {
            var key = new FeedCacheKey(symbol, frame, priceType);
            return GetSeries(key)?.GetFirstRange(from, to);
        }

        private KeyRange<DateTime> GetLastRange(string symbol, Api.TimeFrames frame, Api.BarPriceType? priceType, DateTime from, DateTime to)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, frame, priceType);
            return GetSeries(key)?.GetFirstRange(from, to);
        }

        private Slice<DateTime, BarEntity> GetFirstBarSlice(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, frame, priceType);
            return GetSeries<BarEntity>(key)?.GetFirstSlice(from, to);
        }

        private IEnumerable<KeyRange<DateTime>> IterateCacheKeysInternal(FeedCacheKey key)
        {
            return IterateCacheKeysInternal(key, DateTime.MinValue, DateTime.MaxValue);
        }

        private IEnumerable<KeyRange<DateTime>> IterateCacheKeysInternal(FeedCacheKey key, DateTime from, DateTime to)
        {
            return GetSeries(key)?.IterateRanges(from, to) ?? Enumerable.Empty<KeyRange<DateTime>>();
        }

        private void IterateCacheKeysInternal(Channel<KeyRange<DateTime>> channel, FeedCacheKey key, DateTime from, DateTime to)
        {
            channel.WriteAll(() => IterateCacheKeysInternal(key, from, to));
        }

        protected void IterateBarCache(Channel<Slice<DateTime, BarEntity>> channel, FeedCacheKey key, DateTime from, DateTime to)
        {
            channel.WriteAll(() => IterateBarCacheInternal(key, from, to));
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

        protected Slice<DateTime, BarEntity> QueryBarCache(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, frame, priceType);
            return GetSeries<BarEntity>(key)?.GetFirstSlice(from, to);
        }

        protected void Put(FeedCacheKey key, DateTime from, DateTime to, BarEntity[] values)
        {
            CheckState();

            var collection = GetSeries<BarEntity>(key, true);
            collection.Write(from, to, values);
        }

        protected void Put(string symbol, Api.TimeFrames frame, Api.BarPriceType priceType, DateTime from, DateTime to, BarEntity[] values)
        {
            Put(new FeedCacheKey(symbol, frame, priceType), from, to, values);
        }

        #endregion

        #region Tick History

        //private IEnumerable<Slice<DateTime, QuoteEntity>> IterateTickCache(FeedCacheKey key, DateTime from, DateTime to)
        //{
        //    return IterateTickCacheInternal(key, from, to).GetSyncWrapper(_syncObj);
        //}

        protected IEnumerable<Slice<DateTime, QuoteEntity>> IterateTickCacheInternal(FeedCacheKey key, DateTime from, DateTime to)
        {
            CheckState();
            foreach (var entry in GetSeries<QuoteEntity>(key)?.IterateSlices(from, to))
            {
                CheckState();
                yield return entry;
            }
        }

        protected void Put(string symbol, Api.TimeFrames timeFrame, DateTime from, DateTime to, QuoteEntity[] values)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, timeFrame);
            var collection = GetSeries<QuoteEntity>(key, true);
            collection.Write(from, to, values);
        }

        protected void Put(string symbol, Api.TimeFrames timeFrame, Slice<DateTime, QuoteEntity> slice)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, timeFrame);
            var collection = GetSeries<QuoteEntity>(key, true);
            collection.Write(slice);
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

        protected void RemoveSeries(FeedCacheKey seriesKey)
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

        protected void CheckState()
        {
            if (_diskStorage == null)
                throw new Exception("Invalid operation! CustomFeedStorage is not started or already stopped!");
        }

        //private IVarSet<FeedCacheKey> GetKeysSyncCopy(ISyncContext context)
        //{
        //    lock (_syncObj)
        //        return new SetSynchronizer<FeedCacheKey>(Keys, context);
        //}

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
