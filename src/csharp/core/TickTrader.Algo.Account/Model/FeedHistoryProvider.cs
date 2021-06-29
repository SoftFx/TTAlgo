using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;

namespace TickTrader.Algo.Account
{
    public class FeedHistoryProviderModel : Actor
    {
        private IAlgoLogger logger;

        private const int SliceMaxSize = 4000;
        private string _dataFolder;
        private FeedHistoryFolderOptions _folderOptions;
        private FeedCache.Handler _diskCache = new FeedCache.Handler(SpawnLocal<FeedCache>());
        private IFeedServerApi _feedProxy;

        private void Init(string onlieDataFolder, FeedHistoryFolderOptions folderOptions, string loggerId)
        {
            logger = AlgoLoggerFactory.GetLogger<FeedHistoryProviderModel>(loggerId);
            _dataFolder = onlieDataFolder;
            _folderOptions = folderOptions;
        }

        internal class ControlHandler : Handler<FeedHistoryProviderModel>
        {
            public ControlHandler(ConnectionModel connection, string onlieDataFolder, FeedHistoryFolderOptions folderOptions, string loggerId)
                : base(SpawnLocal<FeedHistoryProviderModel>())
            {
                Actor.Send(a => a.Init(onlieDataFolder, folderOptions, loggerId));
            }

            public Task Start(IFeedServerApi api, string server, string login) => Actor.Call(a => a.Start(api, server, login));
            public Task Stop() => Actor.Call(a => a.Stop());

            public Ref<FeedHistoryProviderModel> Ref => Actor;
        }

        public class Handler : Handler<FeedHistoryProviderModel>
        {
            public Handler(Ref<FeedHistoryProviderModel> aRef) : base(aRef) { }

            public FeedCache.Handler Cache { get; private set; }

            public async Task Init()
            {
                Cache = await Actor.Call(a => a.Cache);
                await Cache.SyncData();
            }

            /// Warning: This method downloads all bars into a collection of unlimmited size! Use wisely!
            public Task<List<BarData>> GetBarList(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
            {
                return Actor.Call(a => a.GetBarList(symbol, marketSide, timeframe, from, to));
            }

            /// Warning: This method downloads all bars into a collection of unlimmited size! Use wisely!
            public Task<List<QuoteInfo>> GetQuoteList(string symbol, Timestamp from, Timestamp to, bool includeLevel2)
            {
                return Actor.Call(a => a.GetQuoteList(symbol, from, to, includeLevel2));
            }

            public Task<BarData[]> GetBarPage(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp startTime, int count)
            {
                return Actor.Call(a => a.GetBarPage(symbol, marketSide, timeframe, startTime, count));
            }

            public Task<QuoteInfo[]> GetQuotePage(string symbol, Timestamp startTime, int count, bool includeLevel2)
            {
                return Actor.Call(a => a.GetQuotePage(symbol, startTime, count, includeLevel2));
            }

            public Task<Tuple<DateTime?, DateTime?>> GetAvailableRange(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe)
            {
                return Actor.Call(a => a.GetAvailableRange(symbol, marketSide, timeframe));
            }

            public async Task<ActorChannel<SliceInfo>> DownloadBarSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
            {
                if (from.Kind != DateTimeKind.Utc || to.Kind != DateTimeKind.Utc)
                    throw new Exception("FeedHistoryProviderModel accepts only UTC dates!");

                var channel = ActorChannel.NewOutput<SliceInfo>();
                await Actor.OpenChannel(channel, (a, c) => a.DownloadBarSeriesToStorage(c, symbol, timeframe, marketSide, Prepare(from), Prepare(to)));
                return channel;
            }

            public async Task<ActorChannel<SliceInfo>> DownloadTickSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, DateTime from, DateTime to)
            {
                if (from.Kind != DateTimeKind.Utc || to.Kind != DateTimeKind.Utc)
                    throw new Exception("FeedHistoryProviderModel accepts only UTC dates!");

                var channel = ActorChannel.NewOutput<SliceInfo>();
                await Actor.OpenChannel(channel, (a, c) => a.DownloadTickSeriesToStorage(c, symbol, timeframe, Prepare(from), Prepare(to)));
                return channel;
            }

            private static DateTime Prepare(DateTime dateTime)
            {
                return dateTime.ToUniversalTime();
            }
        }

        protected FeedCache.Handler Cache => _diskCache;

        private async Task Start(IFeedServerApi feed, string server, string login)
        {
            _feedProxy = feed;

            var onlineFolder = _dataFolder;
            if (_folderOptions == FeedHistoryFolderOptions.ServerHierarchy || _folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(server));
            if (_folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(login));

            //await _diskCache.SyncData();
            await _diskCache.Start(onlineFolder);
        }

        private async Task Stop()
        {
            try
            {
                _feedProxy = null;
                await _diskCache.Stop();
                //_diskCache.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error("Init ERROR " + ex.ToString());
            }
        }

        private Task<Tuple<DateTime?, DateTime?>> GetAvailableRange(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe)
        {
            return _feedProxy?.GetAvailableRange(symbol, marketSide, timeframe) ?? Task.FromResult<Tuple<DateTime?, DateTime?>>(null);
        }

        private async Task<BarData[]> GetBarPage(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int pageSize)
        {
            var pages = new List<BarData[]>();

            var isBackward = pageSize < 0;
            pageSize = Math.Abs(pageSize);

            while (pageSize > 0)
            {
                if (!isBackward && from > DateTime.UtcNow.ToTimestamp())
                    break; // we get last bar somehow even it is out of our requested frame

                var page = await _feedProxy.DownloadBarPage(symbol, from, isBackward ? -pageSize : pageSize, marketSide, timeframe);

                if (page.Length == 0)
                    break;

                pages.Add(page);
                pageSize -= page.Length;

                from = isBackward
                    ? page.First().OpenTime.AddMilliseconds(-1)
                    : page.Last().CloseTime.AddMilliseconds(1);
            }

            return pages.ConcatAll();
        }

        private async Task<QuoteInfo[]> GetQuotePage(string symbol, Timestamp from, int count, bool includeLevel2)
        {
            var pages = new List<QuoteInfo[]>();

            var isBackward = count < 0;
            count = Math.Abs(count);

            while (count > 0)
            {
                if (!isBackward && from > DateTime.UtcNow.ToTimestamp())
                    break; // we get last bar somehow even it is out of our requested frame

                var page = await _feedProxy.DownloadQuotePage(symbol, from, isBackward ? -count : count, includeLevel2);

                if (page.Length == 0)
                    break;

                pages.Add(page);
                count -= page.Length;

                from = isBackward
                    ? page.First().Timestamp.AddMilliseconds(-1)
                    : page.Last().Timestamp.AddMilliseconds(1);
            }

            return pages.ConcatAll();
        }

        private async Task<List<BarData>> GetBarList(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            var result = new List<BarData>();

            while (true)
            {
                var page = await _feedProxy.DownloadBarPage(symbol, from, 4000, marketSide, timeframe);

                if (page == null || page.Length == 0)
                    return result;

                logger.Debug("Downloaded bar page {0} : {1} ({2} {3} {4})", from, page.Length, symbol, marketSide, timeframe);

                foreach (var bar in page)
                {
                    if (bar.OpenTime <= to)
                    {
                        result.Add(bar);
                        from = bar.CloseTime;
                    }
                    else
                        return result;
                }

                if (page.Length < 5)
                    return result;
            }
        }

        private async Task<List<QuoteInfo>> GetQuoteList(string symbol, Timestamp from, Timestamp to, bool includeLevel2)
        {
            var result = new List<QuoteInfo>();

            while (true)
            {
                var page = await _feedProxy.DownloadQuotePage(symbol, from, 4000, includeLevel2);

                if (page == null || page.Length == 0)
                    return result;

                logger.Debug("Downloaded quote page {0} : {1} ({2} {3})", from, page.Length, symbol, includeLevel2 ? "l2" : "top");

                foreach (var quote in page)
                {
                    if (quote.Timestamp <= to)
                    {
                        result.Add(quote);
                        from = quote.Timestamp;
                    }
                    else
                        return result;
                }

                if (page.Length < 5)
                    return result;
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

        private async Task<DateTime> DownloadBarsInternal(Func<Slice<BarData>, IAwaitable<bool>> outputAction, FeedCacheKey key, DateTime from, DateTime to)
        {
            var inputStream = ActorChannel.NewInput<BarData>();
            var barSlicer = TimeSlicer.GetBarSlicer(SliceMaxSize, from, to);

            logger.Debug("start downloading bars (" + from + " - " + to + ")");

            var correctedTo = to - TimeSpan.FromTicks(1);
            var hasData = false;

            try
            {
                _feedProxy.DownloadBars(CreateBlockingChannel(inputStream), key.Symbol, from.ToTimestamp(), correctedTo.ToTimestamp(), key.MarketSide.Value, key.TimeFrame);

                var i = from;
                while (await inputStream.ReadNext())
                {
                    if (barSlicer.Write(inputStream.Current))
                    {
                        var slice = barSlicer.CompleteSlice(false);

                        logger.Debug("downloaded slice {0} - {1}", slice.From, slice.To);

                        //var slice = new BarStreamSlice(i, sliceTo, bars);
                        await Cache.Put(key, slice.From, slice.To, slice.Items);

                        hasData = true;

                        if (!await outputAction(slice))
                        {
                            logger.Debug("Downloading canceled!");
                            throw new TaskCanceledException();
                        }
                        i = slice.To;
                    }
                }

                var lastSlice = barSlicer.CompleteSlice(true);
                if (lastSlice != null)
                {
                    logger.Debug("downloaded slice {0} - {1}", lastSlice.From, lastSlice.To);
                    await Cache.Put(key, lastSlice.From, lastSlice.To, lastSlice.Items);

                    hasData = true;

                    if (!await outputAction(lastSlice))
                    {
                        logger.Debug("Downloading canceled!");
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

        private Task<DateTime> DownloadTicksInternal(ActorChannel<Slice<QuoteInfo>> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadTicksInternal(s => buffer.Write(s), key, from, to);
        }

        private Task<DateTime> DownloadTicksInternal(ActorChannel<SliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadTicksInternal(s => buffer.Write(s), key, from, to);
        }

        private async Task<DateTime> DownloadTicksInternal(Func<Slice<QuoteInfo>, IAwaitable<bool>> outputAction, FeedCacheKey key, DateTime from, DateTime to)
        {
            var level2 = key.TimeFrame == Feed.Types.Timeframe.TicksLevel2;
            var inputStream = ActorChannel.NewInput<QuoteInfo>();
            var quoteSlicer = TimeSlicer.GetQuoteSlicer(SliceMaxSize, from, to);
            var hasData = false;

            logger.Debug("Start downloading quotes (" + from + " - " + to + ")");

            try
            {
                _feedProxy.DownloadQuotes(CreateBlockingChannel(inputStream), key.Symbol, from.ToTimestamp(), to.ToTimestamp(), level2);

                var i = from;
                while (await inputStream.ReadNext())
                {
                    if (quoteSlicer.Write(inputStream.Current))
                    {
                        var slice = quoteSlicer.CompleteSlice(false);

                        logger.Debug("downloaded slice {0} - {1}", slice.From, slice.To);

                        //var slice = new BarStreamSlice(i, sliceTo, bars);
                        await Cache.Put(key, slice.From, slice.To, slice.Items);

                        hasData = true;

                        if (!await outputAction(slice))
                        {
                            logger.Debug("Downloading canceled!");
                            throw new TaskCanceledException();
                        }
                        i = slice.To;
                    }
                }

                var lastSlice = quoteSlicer.CompleteSlice(true);
                if (lastSlice != null)
                {
                    logger.Debug("downloaded slice {0} - {1}", lastSlice.From, lastSlice.To);
                    await Cache.Put(key, lastSlice.From, lastSlice.To, lastSlice.Items);

                    hasData = true;

                    if (!await outputAction(lastSlice))
                    {
                        logger.Debug("Downloading canceled!");
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
            return Cache.Put(key, from, to, new BarData[0]);
        }

        private Task WriteEmptyQuoteSegment(FeedCacheKey key, DateTime from, DateTime to)
        {
            return Cache.Put(key, from, to, new QuoteInfo[0]);
        }
    }

    public enum FeedHistoryFolderOptions
    {
        NoHierarchy, // places history right into specified folder
        ServerHierarchy, // creates subfolder for server
        ServerClientHierarchy // creates subfolder for server and nested subfolder for client.
    }
}
