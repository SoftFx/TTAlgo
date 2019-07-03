using ActorSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.SeriesStorage;
using TickTrader.Server.QuoteHistory.Serialization;
using TT = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Common.Model
{
    public class FeedHistoryProviderModel : Actor
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<FeedHistoryProviderModel>();

        private const int SliceMaxSize = 4000;
        private string _dataFolder;
        private FeedHistoryFolderOptions _folderOptions;
        private FeedCache.Handler _diskCache = new FeedCache.Handler(SpawnLocal<FeedCache>());
        private IFeedServerApi _feedProxy;

        private void Init(string onlieDataFolder, FeedHistoryFolderOptions folderOptions)
        {
            _dataFolder = onlieDataFolder;
            _folderOptions = folderOptions;
        }

        internal class ControlHandler : Handler<FeedHistoryProviderModel>
        {
            public ControlHandler(ConnectionModel connection, string onlieDataFolder, FeedHistoryFolderOptions folderOptions)
                : base(SpawnLocal<FeedHistoryProviderModel>())
            {
                Actor.Send(a => a.Init(onlieDataFolder, folderOptions));
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
            public Task<List<BarEntity>> GetBarList(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime from, DateTime to)
            {
                return Actor.Call(a => a.GetBarList(symbol, priceType, timeFrame, Prepare(from), Prepare(to)));
            }

            /// Warning: This method downloads all bars into a collection of unlimmited size! Use wisely!
            public Task<List<QuoteEntity>> GetQuoteList(string symbol, DateTime from, DateTime to, bool includeLevel2)
            {
                return Actor.Call(a => a.GetQuoteList(symbol, Prepare(from), Prepare(to), includeLevel2));
            }

            public Task<BarEntity[]> GetBarPage(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int count)
            {
                return Actor.Call(a => a.GetBarPage(symbol, priceType, timeFrame, Prepare(startTime), count));
            }

            public Task<QuoteEntity[]> GetQuotePage(string symbol, DateTime startTime, int count, bool includeLevel2)
            {
                return Actor.Call(a => a.GetQuotePage(symbol, Prepare(startTime), count, includeLevel2));
            }

            public Task<Tuple<DateTime?, DateTime?>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
            {
                return Actor.Call(a => a.GetAvailableRange(symbol, priceType, timeFrame));
            }

            public async Task<Channel<SliceInfo>> DownloadBarSeriesToStorage(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
            {
                if (from.Kind != DateTimeKind.Utc || to.Kind != DateTimeKind.Utc)
                    throw new Exception("FeedHistoryProviderModel accepts only UTC dates!");

                var channel = Channel.NewOutput<SliceInfo>();
                await Actor.OpenChannel(channel, (a, c) => a.DownloadBarSeriesToStorage(c, symbol, timeFrame, priceType, Prepare(from), Prepare(to)));
                return channel;
            }

            public async Task<Channel<SliceInfo>> DownloadTickSeriesToStorage(string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
            {
                if (from.Kind != DateTimeKind.Utc || to.Kind != DateTimeKind.Utc)
                    throw new Exception("FeedHistoryProviderModel accepts only UTC dates!");

                var channel = Channel.NewOutput<SliceInfo>();
                await Actor.OpenChannel(channel, (a, c) => a.DownloadTickSeriesToStorage(c, symbol, timeFrame, Prepare(from), Prepare(to)));
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

        private Task<Tuple<DateTime?, DateTime?>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
        {
            return _feedProxy?.GetAvailableRange(symbol, priceType, timeFrame) ?? Task.FromResult<Tuple<DateTime?, DateTime?>>(null);
        }

        private async Task<BarEntity[]> GetBarPage(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int pageSize)
        {
            var pages = new List<BarEntity[]>();

            var from = startTime.ToUniversalTime();
            var isBackward = pageSize < 0;
            pageSize = Math.Abs(pageSize);

            while (pageSize > 0)
            {
                if (!isBackward && from > DateTime.UtcNow)
                    break; // we get last bar somehow even it is out of our requested frame

                var page = await _feedProxy.DownloadBarPage(symbol, from, isBackward ? -pageSize : pageSize, priceType, timeFrame);

                if (page.Length == 0)
                    break;

                pages.Add(page);
                pageSize -= page.Length;

                from = isBackward ? page.First().OpenTime.AddMilliseconds(-1) : page.Last().CloseTime.AddMilliseconds(1);
            }

            return pages.ConcatAll();
        }

        private async Task<QuoteEntity[]> GetQuotePage(string symbol, DateTime startTime, int count, bool includeLevel2)
        {
            var pages = new List<QuoteEntity[]>();

            var from = startTime.ToUniversalTime();
            var isBackward = count < 0;
            count = Math.Abs(count);

            while (count > 0)
            {
                if (!isBackward && from > DateTime.UtcNow)
                    break; // we get last bar somehow even it is out of our requested frame

                var page = await _feedProxy.DownloadQuotePage(symbol, startTime, isBackward ? -count : count, includeLevel2);

                if (page.Length == 0)
                    break;

                pages.Add(page);
                count -= page.Length;

                from = isBackward ? page.First().Time.AddMilliseconds(-1) : page.Last().Time.AddMilliseconds(1);
            }

            return pages.ConcatAll();
        }

        private async Task<List<BarEntity>> GetBarList(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime from, DateTime to)
        {
            var result = new List<BarEntity>();

            while (true)
            {
                var page = await _feedProxy.DownloadBarPage(symbol, from, 4000, priceType, timeFrame);

                if (page == null || page.Length == 0)
                    return result;

                logger.Debug("Downloaded bar page {0} : {1} ({2} {3} {4})", from, page.Length, symbol, timeFrame, priceType);

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

        private async Task<List<QuoteEntity>> GetQuoteList(string symbol, DateTime from, DateTime to, bool includeLevel2)
        {
            var result = new List<QuoteEntity>();

            while (true)
            {
                var page = await _feedProxy.DownloadQuotePage(symbol, from, 4000, includeLevel2);

                if (page == null || page.Length == 0)
                    return result;

                logger.Debug("Downloaded quote page {0} : {1} ({2} {3})", from, page.Length, symbol, includeLevel2 ? "l2" : "top");

                foreach (var quote in page)
                {
                    if (quote.Time <= to)
                    {
                        result.Add(quote);
                        from = quote.Time;
                    }
                    else
                        return result;
                }

                if (page.Length < 5)
                    return result;
            }
        }

        private void DownloadBarSeriesToStorage(Channel<SliceInfo> stream, string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            GetSeriesData(stream, symbol, timeFrame, priceType, from, to, GetCacheInfo, DownloadBarsInternal);
        }

        private void DownloadTickSeriesToStorage(Channel<SliceInfo> stream, string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
        {
            GetSeriesData(stream, symbol, timeFrame, null, from, to, GetCacheInfo, DownloadTicksInternal);
        }

        private IAsyncReader<SliceInfo> GetCacheInfo(FeedCacheKey key, DateTime from, DateTime to)
        {
            return _diskCache.IterateCacheKeys(key, from, to).Select(s => new SliceInfo(s.From, s.To, 0));
        }

        private Task<DateTime> DownloadBarsInternal(Channel<Slice<BarEntity>> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadBarsInternal(s => buffer.Write(s), key, from, to);
        }

        private Task<DateTime> DownloadBarsInternal(Channel<SliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadBarsInternal(s => buffer.Write(s), key, from, to);
        }

        private async Task<DateTime> DownloadBarsInternal(Func<Slice<BarEntity>, IAwaitable<bool>> outputAction, FeedCacheKey key, DateTime from, DateTime to)
        {
            var inputStream = Channel.NewInput<BarEntity>();
            var barSlicer = TimeSlicer.GetBarSlicer(SliceMaxSize, from, to);

            logger.Debug("start downloading bars (" + from + " - " + to + ")");

            var correctedTo = to - TimeSpan.FromTicks(1);
            var hasData = false;

            try
            {
                _feedProxy.DownloadBars(CreateBlockingChannel(inputStream), key.Symbol, from, correctedTo, key.PriceType.Value, key.Frame);

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

        private Task<DateTime> DownloadTicksInternal(Channel<Slice<QuoteEntity>> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadTicksInternal(s => buffer.Write(s), key, from, to);
        }

        private Task<DateTime> DownloadTicksInternal(Channel<SliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadTicksInternal(s => buffer.Write(s), key, from, to);
        }

        private async Task<DateTime> DownloadTicksInternal(Func<Slice<QuoteEntity>, IAwaitable<bool>> outputAction, FeedCacheKey key, DateTime from, DateTime to)
        {
            var level2 = key.Frame == TimeFrames.TicksLevel2;
            var inputStream = Channel.NewInput<QuoteEntity>();
            var quoteSlicer = TimeSlicer.GetQuoteSlicer(SliceMaxSize, from, to);
            var hasData = false;

            logger.Debug("Start downloading quotes (" + from + " - " + to + ")");

            try
            {
                _feedProxy.DownloadQuotes(CreateBlockingChannel(inputStream), key.Symbol, from, to, level2);

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

        private async void GetSeriesData<TOut>(Channel<TOut> buffer,
            string symbol, TimeFrames timeFrame, BarPriceType? priceType, DateTime from, DateTime to,
            Func<FeedCacheKey, DateTime, DateTime, IAsyncReader<TOut>> cacheProvider,
            Func<Channel<TOut>, FeedCacheKey, DateTime, DateTime, Task<DateTime>> download)
            where TOut : SliceInfo
        {
            try
            {
                var key = new FeedCacheKey(symbol, timeFrame, priceType);
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
            return Cache.Put(key, from, to, new BarEntity[0]);
        }

        private Task WriteEmptyQuoteSegment(FeedCacheKey key, DateTime from, DateTime to)
        {
            return Cache.Put(key, from, to, new QuoteEntity[0]);
        }
    }

    public enum FeedHistoryFolderOptions
    {
        NoHierarchy, // places history right into specified folder
        ServerHierarchy, // creates subfolder for server
        ServerClientHierarchy // creates subfolder for server and nested subfolder for client.
    }
}
