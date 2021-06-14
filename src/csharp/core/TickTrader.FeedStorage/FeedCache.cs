﻿using ActorSharp;
using ActorSharp.Lib;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.Lmdb;

namespace TickTrader.FeedStorage
{
    public class FeedCache : Actor
    {
        private readonly ActorEvent<FeedCacheKey> _addListeners = new ActorEvent<FeedCacheKey>();
        private readonly ActorEvent<FeedCacheKey> _removeListeners = new ActorEvent<FeedCacheKey>();

        private readonly VarDictionary<FeedCacheKey, ISeriesStorage<DateTime>> _series = new VarDictionary<FeedCacheKey, ISeriesStorage<DateTime>>();

        protected IVarSet<FeedCacheKey> Keys => _series.Keys;

        protected ISeriesDatabase Database { get; private set; }


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
            private string _baseFolder;
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
                RemoveCallback = ActorCallback.Create<FeedCacheKey>(k => _series.Remove(k));

                var snapshot = await _ref.Call(a =>
                {
                    a._addListeners.Add(addCallback);
                    a._removeListeners.Add(RemoveCallback);
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
                        a._removeListeners.Remove(RemoveCallback);
                    });
                }
            }

            public IVarSet<FeedCacheKey> Keys => _series?.Keys;

            public ActorCallback<FeedCacheKey> RemoveCallback { get => removeCallback; set => removeCallback = value; }

            public Task Start(string folder)
            {
                _baseFolder = folder;
                return _ref.Call(a => a.Start(folder));
            }

            //public Task Refresh() => _ref.Call(r => r.Refresh());

            public Task Stop() => _ref.Call(a => a.Stop());

            public Task Put(FeedCacheKey key, DateTime from, DateTime to, QuoteInfo[] values)
                => Put(key.Symbol, key.TimeFrame, from, to, values);

            public Task Put(string symbol, Feed.Types.Timeframe timeframe, DateTime from, DateTime to, QuoteInfo[] values)
                => _ref.Call(a => a.Put(symbol, timeframe, from, to, values));

            public Task Put(string symbol, Feed.Types.Timeframe timeframe, Slice<DateTime, QuoteInfo> slice)
                => _ref.Call(a => a.Put(symbol, timeframe, slice));

            public Task Put(FeedCacheKey key, DateTime from, DateTime to, BarData[] values)
                => _ref.Call(a => a.Put(key, from, to, values));

            public Task Put(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide marketSide, DateTime from, DateTime to, BarData[] values)
                => Put(new FeedCacheKey(symbol, frame, marketSide), from, to, values);

            public ActorChannel<Slice<DateTime, BarData>> IterateBarCacheAsync(FeedCacheKey key, DateTime from, DateTime to)
            {
                var channel = new ActorChannel<Slice<DateTime, BarData>>(ChannelDirections.Out, 1);
                _ref.SendChannel(channel, (a, c) => a.IterateBarCache(c, key, from, to));
                return channel;
            }

            public ActorChannel<Slice<DateTime, QuoteInfo>> IterateTickCacheAsync(FeedCacheKey key, DateTime from, DateTime to)
            {
                var channel = new ActorChannel<Slice<DateTime, QuoteInfo>>(ChannelDirections.Out, 1);
                _ref.SendChannel(channel, (a, c) => a.IterateTickCache(c, key, from, to));
                return channel;
            }

            public BlockingChannel<Slice<DateTime, BarData>> IterateBarCache(FeedCacheKey key, DateTime from, DateTime to)
            {
                return _ref.OpenBlockingChannel<FeedCache, Slice<DateTime, BarData>>(ChannelDirections.Out, 2, (a, c) => a.IterateBarCache(c, key, from, to));
            }

            public BlockingChannel<Slice<DateTime, QuoteInfo>> IterateTickCache(FeedCacheKey key, DateTime from, DateTime to)
            {
                return _ref.OpenBlockingChannel<FeedCache, Slice<DateTime, QuoteInfo>>(ChannelDirections.Out, 2, (a, c) => a.IterateTickCache(c, key, from, to));
            }

            public ActorChannel<KeyRange<DateTime>> IterateCacheKeys(FeedCacheKey key, DateTime from, DateTime to)
            {
                var channel = new ActorChannel<KeyRange<DateTime>>(ChannelDirections.Out, 1);
                _ref.SendChannel(channel, (a, c) => a.IterateCacheKeys(c, key, from, to));
                return channel;
            }

            public Task<KeyRange<DateTime>> GetFirstRange(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide? marketSide, DateTime from, DateTime to)
                => _ref.Call(a => a.GetFirstRange(symbol, frame, marketSide, from, to));

            public Task<Tuple<DateTime?, DateTime?>> GetRange(FeedCacheKey key)
                => _ref.Call(a => a.GetRange(key));

            public Task<double?> GetCollectionSize(FeedCacheKey key)
                => _ref.Call(a => a.GetCollectionSize(key));

            public Task RemoveSeries(FeedCacheKey seriesKey)
                => _ref.Call(a => a.RemoveSeries(seriesKey));

            public BarCrossDomainReader CreateBarCrossDomainReader(CrossDomainReaderRequest request)
            {
                return new BarCrossDomainReader(_baseFolder, request);
            }

            public TickCrossDomainReader CreateTickCrossDomainReader(CrossDomainReaderRequest request)
            {
                return new TickCrossDomainReader(_baseFolder, request);
            }

            [Conditional("DEBUG")]
            public void PrintSlices(FeedCacheKey key) => _ref.Send(a => a.PrintSlices(key));
        }

        protected virtual void Start(string folder)
        {
            if (Database != null)
                throw new InvalidOperationException("Already started!");

            Database = SeriesDatabase.Create(new LmdbManager(folder));

            Refresh();
        }

        protected virtual void Refresh()
        {
            _series.Clear();

            var loadedKeys = new List<FeedCacheKey>();

            foreach (var collectionName in Database.Collections)
            {
                if (!IsSpecialCollection(collectionName))
                {
                    if (FeedCacheKey.TryParse(collectionName, out var key))
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
            if (Database != null)
            {
                Database.Dispose();
                Database = null;
            }
        }

        protected Tuple<DateTime?, DateTime?> GetRange(FeedCacheKey key)
        {
            CheckState();

            DateTime? min = null;
            DateTime? max = null;

            foreach (var r in IterateCacheKeysInternal(key))
            {
                if (min == null)
                    min = r.From;

                max = r.To;
            }

            return new Tuple<DateTime?, DateTime?>(min, max);
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

        private void IterateCacheKeys(ActorChannel<KeyRange<DateTime>> channel, FeedCacheKey key, DateTime from, DateTime to)
        {
            channel.WriteAll(() => IterateCacheKeysInternal(key, from, to));
        }

        #region Bar History

        private KeyRange<DateTime> GetFirstRange(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide? marketSide, DateTime from, DateTime to)
        {
            var key = new FeedCacheKey(symbol, frame, marketSide);
            return GetSeries(key)?.GetFirstRange(from, to);
        }

        private KeyRange<DateTime> GetLastRange(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide? marketSide, DateTime from, DateTime to)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, frame, marketSide);
            return GetSeries(key)?.GetFirstRange(from, to);
        }

        private Slice<DateTime, BarData> GetFirstBarSlice(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, frame, marketSide);
            return GetSeries<BarData>(key)?.GetFirstSlice(from, to);
        }

        private IEnumerable<KeyRange<DateTime>> IterateCacheKeysInternal(FeedCacheKey key)
        {
            return IterateCacheKeysInternal(key, DateTime.MinValue, DateTime.MaxValue);
        }

        private IEnumerable<KeyRange<DateTime>> IterateCacheKeysInternal(FeedCacheKey cacheId, DateTime from, DateTime to)
        {
            for (var i = from; i < to;)
            {
                var page = ReadKeyPage(cacheId, i, to, 20);

                if (page.Count == 0)
                    break;

                foreach (var key in page)
                {
                    yield return key;
                    i = key.To;
                }
            }
        }

        private List<KeyRange<DateTime>> ReadKeyPage(FeedCacheKey key, DateTime from, DateTime to, int pageSize)
        {
            var series = GetSeries(key);
            if (series != null)
                return series.IterateRanges(from, to).Take(pageSize).ToList();
            return new List<KeyRange<DateTime>>();
        }

        private void IterateCacheKeysInternal(ActorChannel<KeyRange<DateTime>> channel, FeedCacheKey key, DateTime from, DateTime to)
        {
            channel.WriteAll(() => IterateCacheKeysInternal(key, from, to));
        }

        protected void IterateBarCache(ActorChannel<Slice<DateTime, BarData>> channel, FeedCacheKey key, DateTime from, DateTime to)
        {
            channel.WriteAll(() => IterateBarCacheInternal(key, from, to));
        }

        private IEnumerable<Slice<DateTime, BarData>> IterateBarCacheInternal(FeedCacheKey key, DateTime from, DateTime to)
        {
            CheckState();
            foreach (var entry in GetSeries<BarData>(key)?.IterateSlices(from, to))
            {
                CheckState();
                yield return entry;
            }
        }

        protected Slice<DateTime, BarData> QueryBarCache(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, frame, marketSide);
            return GetSeries<BarData>(key)?.GetFirstSlice(from, to);
        }

        protected void Put(FeedCacheKey key, DateTime from, DateTime to, BarData[] values)
        {
            CheckState();

            var collection = GetSeries<BarData>(key, true);
            collection.Write(from, to, values);
        }

        protected void Put(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide marketSide, DateTime from, DateTime to, BarData[] values)
        {
            Put(new FeedCacheKey(symbol, frame, marketSide), from, to, values);
        }

        #endregion

        #region Tick History

        private void IterateTickCache(ActorChannel<Slice<DateTime, QuoteInfo>> channel, FeedCacheKey key, DateTime from, DateTime to)
        {
            channel.WriteAll(() => IterateTickCacheInternal(key, from, to));
        }

        protected IEnumerable<Slice<DateTime, QuoteInfo>> IterateTickCacheInternal(FeedCacheKey key, DateTime from, DateTime to)
        {
            CheckState();
            foreach (var entry in GetSeries<QuoteInfo>(key)?.IterateSlices(from, to))
            {
                CheckState();
                yield return entry;
            }
        }

        protected void Put(string symbol, Feed.Types.Timeframe timeFrame, DateTime from, DateTime to, QuoteInfo[] values)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, timeFrame);
            var collection = GetSeries<QuoteInfo>(key, true);
            collection.Write(from, to, values);
        }

        protected void Put(string symbol, Feed.Types.Timeframe timeFrame, Slice<DateTime, QuoteInfo> slice)
        {
            CheckState();

            var key = new FeedCacheKey(symbol, timeFrame);
            var collection = GetSeries<QuoteInfo>(key, true);
            collection.Write(slice);
        }

        #endregion

        private ISeriesStorage<DateTime> CreateCollection(FeedCacheKey key)
        {
            ISeriesStorage<DateTime> collection;

            if (key.TimeFrame == Feed.Types.Timeframe.Ticks || key.TimeFrame == Feed.Types.Timeframe.TicksLevel2)
                collection = Database.GetSeries(new DateTimeKeySerializer(), TickSerializer.GetSerializer(key), b => b.Time, key.CodeString, true);
            else
                collection = Database.GetSeries(new DateTimeKeySerializer(), new BarSerializer(key.TimeFrame), b => b.OpenTime.ToDateTime(), key.CodeString, false);

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
            if (Database == null)
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

        [Conditional("DEBUG")]
        private void PrintSlices(FeedCacheKey key)
        {
            try
            {
                Debug.WriteLine("Cache data " + key.CodeString + ":");

                var count = 0;
                var group = new List<KeyRange<DateTime>>();

                foreach (var slice in IterateCacheKeysInternal(key, DateTime.MinValue, DateTime.MaxValue))
                {
                    var last = group.LastOrDefault();
                    if (last != null && last.To != slice.From)
                    {
                        PrintGroup(group);
                        group.Clear();
                    }

                    group.Add(slice);
                    count++;
                }

                if (group.Count > 0)
                    PrintGroup(group);

                if (count == 0)
                    Debug.WriteLine("Series cache is empty.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        [Conditional("DEBUG")]
        private void PrintGroup(List<KeyRange<DateTime>> slices)
        {
            var from = slices[0].From;
            var to = slices.Last().To;

            Debug.Write("Cluster " + from + " - " + to + ":");
            foreach (var slice in slices)
                Debug.Write(" [" + slice.From + "- " + slice.To + "]");

            Debug.WriteLine("");
        }
    }
}