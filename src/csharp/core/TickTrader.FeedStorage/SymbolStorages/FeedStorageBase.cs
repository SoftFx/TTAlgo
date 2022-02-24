using ActorSharp;
using ActorSharp.Lib;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;
using TickTrader.FeedStorage.Serializers;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage
{
    internal abstract class FeedStorageBase : Actor
    {
        private readonly VarDictionary<FeedCacheKey, ISeriesStorage<DateTime>> _series = new VarDictionary<FeedCacheKey, ISeriesStorage<DateTime>>();
        private readonly ActorEvent<FeedSeriesUpdate> _seriesListeners = new ActorEvent<FeedSeriesUpdate>();

        protected IVarSet<FeedCacheKey> Keys => _series.Keys;

        protected ISeriesDatabase Database { get; private set; }


        public FeedStorageBase()
        {
            _series.Updated += SendSeriesUpdates;
        }


        private void SendSeriesUpdates(DictionaryUpdateArgs<FeedCacheKey, ISeriesStorage<DateTime>> args) =>
            _seriesListeners.FireAndForget(new FeedSeriesUpdate(args.Action, args.Key));


        private readonly struct FeedSeriesUpdate
        {
            public DLinqAction Action { get; }

            public FeedCacheKey Key { get; }

            public double SeriesSize { get; }


            internal FeedSeriesUpdate(DLinqAction action, FeedCacheKey key, double size = 0)
            {
                Action = action;
                Key = key;
                SeriesSize = size;
            }
        }


        internal abstract class FeedHandler : ISymbolCollection
        {
            protected readonly VarDictionary<string, BaseSymbol> _symbols = new VarDictionary<string, BaseSymbol>();

            private readonly ActorCallback<FeedSeriesUpdate> _seriesChangeCallback;
            private readonly Ref<FeedStorageBase> _ref;


            public event Action<ISymbolData> SymbolAdded;
            public event Action<ISymbolData> SymbolRemoved;
            public event Action<ISymbolData, ISymbolData> SymbolUpdated;


            public List<ISymbolData> Symbols => _symbols.Values.Cast<ISymbolData>().ToList();

            public ISymbolData this[string name] => _symbols.GetOrDefault(name);


            internal bool IsStarted { get; private set; }

            public string StorageFolder { get; private set; }


            public FeedHandler(Ref<FeedStorageBase> actorRef)
            {
                _ref = actorRef;
                _seriesChangeCallback = ActorCallback.Create<FeedSeriesUpdate>(UpdateSeriesHandler);

                _symbols.Updated += SendCollectionUpdates;
            }


            public async Task Start(string folder)
            {
                IsStarted = true;
                StorageFolder = folder;

                await _ref.Call(a => a.OpenDatabase(folder));
                await SyncSymbolCollection();
                await SyncStorageSeries();
            }

            protected abstract Task SyncSymbolCollection();


            public abstract Task<bool> TryAddSymbol(ISymbolInfo symbol);

            public abstract Task<bool> TryUpdateSymbol(ISymbolInfo symbol);

            public abstract Task<bool> TryRemoveSymbol(string name);


            protected async Task SyncStorageSeries()
            {
                var snapshot = await _ref.Call(a =>
                {
                    a._seriesListeners.Add(_seriesChangeCallback);
                    return a._series.Snapshot.Select(u => (u.Key, u.Value.GetSize())).ToList();
                });

                foreach (var item in snapshot)
                    if (_symbols.TryGetValue(item.Key.Symbol, out var symbol))
                        symbol.AddSeries(item.Key, item.Item2);
            }

            public Task Stop()
            {
                _ref?.Send(a => a._seriesListeners.Remove(_seriesChangeCallback));

                IsStarted = false;

                return _ref.Call(a => a.CloseDatabase());
            }

            public virtual void Dispose()
            {
                _symbols.Updated -= SendCollectionUpdates;
            }

            //public Task Put(FeedCacheKey key, DateTime from, DateTime to, QuoteInfo[] values)
            //    => Put(key.Symbol, key.TimeFrame, from, to, values);

            //public Task Put(string symbol, Feed.Types.Timeframe timeframe, DateTime from, DateTime to, QuoteInfo[] values)
            //    => _ref.Call(a => a.Put(symbol, timeframe, from, to, values));

            //public Task Put(string symbol, Feed.Types.Timeframe timeframe, Slice<DateTime, QuoteInfo> slice)
            //    => _ref.Call(a => a.Put(symbol, timeframe, slice));

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
                return _ref.OpenBlockingChannel<FeedStorageBase, Slice<DateTime, BarData>>(ChannelDirections.Out, 2, (a, c) => a.IterateBarCache(c, key, from, to));
            }

            public BlockingChannel<Slice<DateTime, QuoteInfo>> IterateTickCache(FeedCacheKey key, DateTime from, DateTime to)
            {
                return _ref.OpenBlockingChannel<FeedStorageBase, Slice<DateTime, QuoteInfo>>(ChannelDirections.Out, 2, (a, c) => a.IterateTickCache(c, key, from, to));
            }

            public ActorChannel<KeyRange<DateTime>> IterateCacheKeys(FeedCacheKey key, DateTime from, DateTime to)
            {
                var channel = new ActorChannel<KeyRange<DateTime>>(ChannelDirections.Out, 1);
                _ref.SendChannel(channel, (a, c) => a.IterateCacheKeys(c, key, from, to));
                return channel;
            }

            //public Task<KeyRange<DateTime>> GetFirstRange(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide? marketSide, DateTime from, DateTime to)
            //    => _ref.Call(a => a.GetFirstRange(symbol, frame, marketSide, from, to));


            public Task<(DateTime?, DateTime?)> GetRange(FeedCacheKey key) => _ref.Call(a => a.GetRange(key));

            //public Task<double?> GetCollectionSize(FeedCacheKey key) => _ref.Call(a => a.GetCollectionSize(key));

            public Task<bool> RemoveSeries(FeedCacheKey seriesKey) => _ref.Call(a => a.RemoveSeries(seriesKey));

            [Conditional("DEBUG")]
            public void PrintSlices(FeedCacheKey key) => _ref.Send(a => a.PrintSlices(key));


            private void UpdateSeriesHandler(FeedSeriesUpdate update)
            {
                if (!_symbols.TryGetValue(update.Key.Symbol, out var symbol))
                    return;

                switch (update.Action)
                {
                    case DLinqAction.Insert:
                        symbol.AddSeries(update.Key);
                        break;
                    case DLinqAction.Remove:
                        symbol.RemoveSeries(update.Key);
                        break;
                    case DLinqAction.Replace:
                        symbol.UpdateSeries(update.Key, update.SeriesSize);
                        break;
                    default:
                        break;
                }
            }

            private void SendCollectionUpdates(DictionaryUpdateArgs<string, BaseSymbol> update)
            {
                switch (update.Action)
                {
                    case DLinqAction.Insert:
                        SymbolAdded?.Invoke(update.NewItem);
                        break;
                    case DLinqAction.Remove:
                        SymbolRemoved?.Invoke(update.OldItem);
                        break;
                    case DLinqAction.Replace:
                        SymbolUpdated?.Invoke(update.OldItem, update.NewItem);
                        break;
                    default:
                        break;
                }
            }
        }


        protected void OpenDatabase(string folder)
        {
            if (Database != null)
                throw new InvalidOperationException("Already started!");

            Database = SeriesDatabase.Create(StorageFactory.BuildBinaryStorage(folder));

            LoadStoredData();
        }

        protected virtual void LoadStoredData(string skippedCollection = null)
        {
            _series.Clear();

            var loadedKeys = new List<FeedCacheKey>(1 << 5);

            foreach (var collectionName in Database.Collections)
                if (collectionName != skippedCollection && FeedCacheKey.TryParse(collectionName, out var key))
                    loadedKeys.Add(key);

            foreach (var key in loadedKeys)
                CreateCollection(key);
        }

        protected virtual void CloseDatabase()
        {
            if (Database != null)
            {
                Database.Dispose();
                Database = null;
            }
        }

        protected (DateTime?, DateTime?) GetRange(FeedCacheKey key)
        {
            CheckState();

            DateTime? min = null;
            DateTime? max = null;

            foreach (var r in IterateCacheKeysInternal(key, DateTime.MinValue, DateTime.MaxValue))
            {
                if (min == null)
                    min = r.From;

                max = r.To;
            }

            return (min, max);
        }

        //protected double? GetCollectionSize(FeedCacheKey key)
        //{
        //    CheckState();

        //    return GetSeries(key)?.GetSize();
        //}

        private void IterateCacheKeys(ActorChannel<KeyRange<DateTime>> channel, FeedCacheKey key, DateTime from, DateTime to)
        {
            channel.WriteAll(() => IterateCacheKeysInternal(key, from, to));
        }

        #region Bar History

        //private KeyRange<DateTime> GetFirstRange(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide? marketSide, DateTime from, DateTime to)
        //{
        //    var key = new FeedCacheKey(symbol, frame, marketSide);
        //    return GetSeries(key)?.GetFirstRange(from, to);
        //}

        //private KeyRange<DateTime> GetLastRange(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide? marketSide, DateTime from, DateTime to)
        //{
        //    CheckState();

        //    var key = new FeedCacheKey(symbol, frame, marketSide);
        //    return GetSeries(key)?.GetFirstRange(from, to);
        //}

        //private Slice<DateTime, BarData> GetFirstBarSlice(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        //{
        //    CheckState();

        //    var key = new FeedCacheKey(symbol, frame, marketSide);
        //    return GetSeries<BarData>(key)?.GetFirstSlice(from, to);
        //}

        protected IEnumerable<KeyRange<DateTime>> IterateCacheKeysInternal(FeedCacheKey cacheId, DateTime from, DateTime to)
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

        //private void IterateCacheKeysInternal(ActorChannel<KeyRange<DateTime>> channel, FeedCacheKey key, DateTime from, DateTime to)
        //{
        //    channel.WriteAll(() => IterateCacheKeysInternal(key, from, to));
        //}

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

        //protected Slice<DateTime, BarData> QueryBarCache(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        //{
        //    CheckState();

        //    var key = new FeedCacheKey(symbol, frame, marketSide);
        //    return GetSeries<BarData>(key)?.GetFirstSlice(from, to);
        //}

        //protected void Put(FeedCacheKey key, DateTime from, DateTime to, BarData[] values)
        //{
        //    CheckState();

        //    var collection = GetSeries<BarData>(key, true);
        //    collection.Write(from, to, values);
        //}

        //protected void Put(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide marketSide, DateTime from, DateTime to, BarData[] values)
        //{
        //    Put(new FeedCacheKey(symbol, frame, marketSide), from, to, values);
        //}

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

        //protected void Put(string symbol, Feed.Types.Timeframe timeFrame, DateTime from, DateTime to, QuoteInfo[] values)
        //{
        //    CheckState();

        //    var key = new FeedCacheKey(symbol, timeFrame);
        //    var collection = GetSeries<QuoteInfo>(key, true);
        //    collection.Write(from, to, values);
        //}

        protected void Put<T>(FeedCacheKey key, DateTime from, DateTime to, T[] values)
        {
            CheckState();

            var collection = GetSeries<T>(key, true);
            collection.Write(from, to, values);

            _seriesListeners.FireAndForget(new FeedSeriesUpdate(DLinqAction.Replace, key, ((ISeriesStorage<DateTime>)collection).GetSize()));
        }

        //protected void Put(string symbol, Feed.Types.Timeframe timeFrame, Slice<DateTime, QuoteInfo> slice)
        //{
        //    CheckState();

        //    var key = new FeedCacheKey(symbol, timeFrame);
        //    var collection = GetSeries<QuoteInfo>(key, true);
        //    collection.Write(slice);
        //}

        #endregion

        private ISeriesStorage<DateTime> CreateCollection(FeedCacheKey key)
        {
            ISeriesStorage<DateTime> collection;

            if (key.TimeFrame.IsTick())
                collection = Database.GetSeries(new DateTimeKeySerializer(), TickSerializer.GetSerializer(key), b => b.Time.ToUniversalTime(), key.FullInfo, true);
            else
                collection = Database.GetSeries(new DateTimeKeySerializer(), new BarSerializer(key.TimeFrame), b => b.OpenTime.ToDateTime(), key.FullInfo, false);

            _series.Add(key, collection);

            return collection;
        }

        protected bool RemoveSeries(FeedCacheKey seriesKey)
        {
            if (_series.TryGetValue(seriesKey, out ISeriesStorage<DateTime> series))
            {
                series.Drop();
                _series.Remove(seriesKey);

                return true;
            }

            return false;
        }

        protected void CheckState()
        {
            if (Database == null)
                throw new Exception("Invalid operation! CustomFeedStorage is not started or already stopped!");
        }

        private ISeriesStorage<DateTime> GetSeries(FeedCacheKey key, bool addIfMissing = false)
        {
            if (!_series.TryGetValue(key, out var series) && addIfMissing)
                series = CreateCollection(key);

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
                Debug.WriteLine("Cache data " + key.FullInfo + ":");

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
        private static void PrintGroup(List<KeyRange<DateTime>> slices)
        {
            var from = slices[0].From;
            var to = slices.Last().To;

            Debug.Write($"Cluster {from} - {to}:");

            foreach (var slice in slices)
                Debug.Write($" [{slice.From} - {slice.To}]");

            Debug.WriteLine(string.Empty);
        }
    }
}
