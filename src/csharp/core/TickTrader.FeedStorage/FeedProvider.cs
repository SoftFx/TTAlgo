using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    internal class FeedProvider : Actor
    {
        private const int SliceMaxSize = 4000;

        private string _dataFolder;

        private OnlineFeedStorage.Handler _diskCache = new OnlineFeedStorage.Handler(SpawnLocal<OnlineFeedStorage>());

        private OnlineFeedStorage.Handler Cache => _diskCache;

        internal IClientFeedProvider FeedProxy { get; private set; }

        //internal SymbolCollection<ISymbolData> Collection { get; private set; }

        //private FeedStorageFolderOptions _folderOptions;
        //private IAlgoLogger logger;


        //public FeedProvider(IClientFeedProvider feed, IOnlineStorageSettings settings)
        //{

        //}

        internal void Init(IClientFeedProvider feed, IOnlineStorageSettings settings)
        {
            var dataFolder = settings.FolderPath;
            var folderOptions = settings.Options;

            var onlineFolder = dataFolder;
            if (folderOptions == FeedStorageFolderOptions.ServerHierarchy || folderOptions == FeedStorageFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathHelper.Escape(settings.Server));
            if (folderOptions == FeedStorageFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathHelper.Escape(settings.Login));

            _dataFolder = onlineFolder;
            FeedProxy = feed;
            //Collection = new SymbolCollection<ISymbolData>(onlineFolder);

            //await _diskCache.SyncData();
            //return _diskCache.Start(onlineFolder);
        }

        private async Task Stop()
        {
            try
            {
                FeedProxy = null;
                await _diskCache.Stop();
                //_diskCache.Dispose();
            }
            catch (Exception ex)
            {
                //logger.Error("Init ERROR " + ex.ToString());
            }
        }

        public class Handler : Handler<FeedProvider>
        {
            public IClientFeedProvider FeedProxy { get; private set; }

            public OnlineFeedStorage.Handler Cache { get; private set; }

            //internal SymbolCollection<ISymbolData> Collection { get; private set; }


            public Handler(IClientFeedProvider feed, IOnlineStorageSettings settings) : base(SpawnLocal<FeedProvider>())
            {
                Actor.Send(a => a.Init(feed, settings));
            }


            public async Task Init()
            {
                Cache = await Actor.Call(a => a._diskCache);
                FeedProxy = await Actor.Call(a => a.FeedProxy);
                //Collection = await Actor.Call(a => a.Collection);

                var folder = await Actor.Call(a => a._dataFolder);

                await Cache.Start(folder);
                //await Cache.SyncData();

                foreach (var s in FeedProxy.Symbols)
                {
                    Cache._symbols.Add(s.Name, new OnlineSymbol(s, this));
                }
                //Cache.Collection.Initialize(FeedProxy.Symbols.Select(s => new OnlineSymbol(s, this)));
            }

            public Task Stop()
            {
                return Actor.Call(a => a.Stop());
            }

            public async Task<ActorChannel<SliceInfo>> DownloadTickSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, DateTime from, DateTime to)
            {
                if (from.Kind != DateTimeKind.Utc || to.Kind != DateTimeKind.Utc)
                    throw new Exception("FeedHistoryProviderModel accepts only UTC dates!");

                var channel = ActorChannel.NewOutput<SliceInfo>();
                await Actor.OpenChannel(channel, (a, c) => a.DownloadTickSeriesToStorage(c, symbol, timeframe, Prepare(from), Prepare(to)));
                return channel;
            }

            public async Task<ActorChannel<SliceInfo>> DownloadBarSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
            {
                if (from.Kind != DateTimeKind.Utc || to.Kind != DateTimeKind.Utc)
                    throw new Exception("FeedHistoryProviderModel accepts only UTC dates!");

                var channel = ActorChannel.NewOutput<SliceInfo>();
                await Actor.OpenChannel(channel, (a, c) => a.DownloadBarSeriesToStorage(c, symbol, timeframe, marketSide, Prepare(from), Prepare(to)));
                return channel;
            }

            private static DateTime Prepare(DateTime dateTime)
            {
                return dateTime.ToUniversalTime();
            }
        }


        private void DownloadBarSeriesToStorage(ActorChannel<SliceInfo> stream, string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        {
            GetSeriesData(stream, symbol, timeframe, marketSide, from, to, GetCacheInfo, DownloadBarsInternal);
        }

        private void DownloadTickSeriesToStorage(ActorChannel<SliceInfo> stream, string symbol, Feed.Types.Timeframe timeframe, DateTime from, DateTime to)
        {
            GetSeriesData(stream, symbol, timeframe, null, from, to, GetCacheInfo, DownloadTicksInternal);
        }

        private IAsyncReader<SliceInfo> GetCacheInfo(FeedCacheKey key, DateTime from, DateTime to)
        {
            return _diskCache.IterateCacheKeys(key, from, to).Select(s => new SliceInfo(s.From, s.To, 0));
        }

        private Task<DateTime> DownloadBarsInternal(ActorChannel<Slice<BarData>> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadBarsInternal(s => buffer.Write(s), key, from, to);
        }

        private Task<DateTime> DownloadBarsInternal(ActorChannel<SliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadBarsInternal(s => buffer.Write(s), key, from, to);
        }

        private Task<DateTime> DownloadTicksInternal(ActorChannel<Slice<QuoteInfo>> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadTicksInternal(s => buffer.Write(s), key, from, to);
        }

        private Task<DateTime> DownloadTicksInternal(ActorChannel<SliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadTicksInternal(s => buffer.Write(s), key, from, to);
        }


        private async Task<DateTime> DownloadBarsInternal(Func<Slice<BarData>, IAwaitable<bool>> outputAction, FeedCacheKey key, DateTime from, DateTime to)
        {
            var inputStream = ActorChannel.NewInput<BarData>();
            var barSlicer = TimeSlicer.GetBarSlicer(SliceMaxSize, from, to);

            //logger.Debug("start downloading bars (" + from + " - " + to + ")");

            var correctedTo = to - TimeSpan.FromTicks(1);
            var hasData = false;

            try
            {
                FeedProxy.DownloadBars(CreateBlockingChannel(inputStream), key.Symbol, from.ToTimestamp(), correctedTo.ToTimestamp(), key.MarketSide.Value, key.TimeFrame);

                var i = from;
                while (await inputStream.ReadNext())
                {
                    if (barSlicer.Write(inputStream.Current))
                    {
                        var slice = barSlicer.CompleteSlice(false);

                        //logger.Debug("downloaded slice {0} - {1}", slice.From, slice.To);

                        //var slice = new BarStreamSlice(i, sliceTo, bars);
                        await Cache.Put(key, slice.From, slice.To, slice.Items);

                        hasData = true;

                        if (!await outputAction(slice))
                        {
                            //logger.Debug("Downloading canceled!");
                            throw new TaskCanceledException();
                        }
                        i = slice.To;
                    }
                }

                var lastSlice = barSlicer.CompleteSlice(true);
                if (lastSlice != null)
                {
                    //logger.Debug("downloaded slice {0} - {1}", lastSlice.From, lastSlice.To);
                    await Cache.Put(key, lastSlice.From, lastSlice.To, lastSlice.Items);

                    hasData = true;

                    if (!await outputAction(lastSlice))
                    {
                        //logger.Debug("Downloading canceled!");
                        throw new TaskCanceledException();
                    }
                    i = lastSlice.To;
                }

                if (!hasData)
                {
                    await WriteEmptyBarSegment(key, from, to);
                    return to;
                }

                return i;
            }
            finally
            {
                await inputStream.Close();
            }
        }

        private async Task<DateTime> DownloadTicksInternal(Func<Slice<QuoteInfo>, IAwaitable<bool>> outputAction, FeedCacheKey key, DateTime from, DateTime to)
        {
            var level2 = key.TimeFrame == Feed.Types.Timeframe.TicksLevel2;
            var inputStream = ActorChannel.NewInput<QuoteInfo>();
            var quoteSlicer = TimeSlicer.GetQuoteSlicer(SliceMaxSize, from, to);
            var hasData = false;

            //logger.Debug("Start downloading quotes (" + from + " - " + to + ")");

            try
            {
                FeedProxy.DownloadQuotes(CreateBlockingChannel(inputStream), key.Symbol, from.ToTimestamp(), to.ToTimestamp(), level2);

                var i = from;
                while (await inputStream.ReadNext())
                {
                    if (quoteSlicer.Write(inputStream.Current))
                    {
                        var slice = quoteSlicer.CompleteSlice(false);

                        //logger.Debug("downloaded slice {0} - {1}", slice.From, slice.To);

                        //var slice = new BarStreamSlice(i, sliceTo, bars);
                        await Cache.Put(key, slice.From, slice.To, slice.Items);

                        hasData = true;

                        if (!await outputAction(slice))
                        {
                            //logger.Debug("Downloading canceled!");
                            throw new TaskCanceledException();
                        }
                        i = slice.To;
                    }
                }

                var lastSlice = quoteSlicer.CompleteSlice(true);
                if (lastSlice != null)
                {
                    //logger.Debug("downloaded slice {0} - {1}", lastSlice.From, lastSlice.To);
                    await Cache.Put(key, lastSlice.From, lastSlice.To, lastSlice.Items);

                    hasData = true;

                    if (!await outputAction(lastSlice))
                    {
                        //logger.Debug("Downloading canceled!");
                        throw new TaskCanceledException();
                    }
                    i = lastSlice.To;
                }

                if (!hasData)
                {
                    await WriteEmptyQuoteSegment(key, from, to);
                    return to;
                }

                return i;
            }
            finally
            {
                await inputStream.Close();
            }
        }

        private async void GetSeriesData<TOut>(ActorChannel<TOut> buffer,
    string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide? marketSide, DateTime from, DateTime to,
    Func<FeedCacheKey, DateTime, DateTime, IAsyncReader<TOut>> cacheProvider,
    Func<ActorChannel<TOut>, FeedCacheKey, DateTime, DateTime, Task<DateTime>> download)
    where TOut : SliceInfo
        {
            try
            {
                var key = new FeedCacheKey(symbol, timeframe, marketSide);
                var i = from;
                var cache = cacheProvider(key, from, to);
                try
                {
                    while (await cache.ReadNext())
                    {
                        var cacheItem = cache.Current;

                        if (cacheItem.From > i)
                            i = await download(buffer, key, i, cacheItem.From);

                        if (!await buffer.Write(cacheItem))
                            return;
                        i = cacheItem.To;
                    }

                    if (i < to)
                        i = await download(buffer, key, i, to);
                }
                finally
                {
                    await cache.Close();
                }
            }
            catch (Exception ex)
            {
                await buffer.Close(ex);
            }
            finally
            {
                await buffer.Close();
            }
        }

        private Task WriteEmptyBarSegment(FeedCacheKey key, DateTime from, DateTime to)
        {
            return _diskCache.Put(key, from, to, new BarData[0]);
        }

        private Task WriteEmptyQuoteSegment(FeedCacheKey key, DateTime from, DateTime to)
        {
            return _diskCache.Put(key, from, to, new QuoteInfo[0]);
        }
    }
}
