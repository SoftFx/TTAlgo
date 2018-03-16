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
using TickTrader.Algo.Core.Math;
using TickTrader.SeriesStorage;
using TickTrader.Server.QuoteHistory.Serialization;
using TT = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Common.Model
{
    public class FeedHistoryProviderModel : Actor
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<FeedHistoryProviderModel>();

        private const int SliceMaxSize = 8000;
        private string _dataFolder;
        private FeedHistoryFolderOptions _folderOptions;
        private FeedCache _diskCache = new FeedCache();
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

            public FeedCache Cache { get; private set; }

            public async Task Init()
            {
                Cache = await Actor.Call(a => a.Cache);
            }

            /// Warning: This method downloads all bars into a collection of unlimmited size! Use wisely!
            public Task<List<BarEntity>> GetBarList(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime from, DateTime to)
            {
                return Actor.Call(a => a.GetBarList(symbol, priceType, timeFrame, from, to));
            }

            public Task<BarEntity[]> GetBarPage(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int count)
            {
                return Actor.Call(a => a.GetBarPage(symbol, priceType, timeFrame, startTime, count));
            }

            public Task<Tuple<DateTime, DateTime>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
            {
                return Actor.Call(a => a.GetAvailableRange(symbol, priceType, timeFrame));
            }

            public async Task<Channel<SliceInfo>> DownloadBarSeriesToStorage(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
            {
                var channel = Channel.NewOutput<SliceInfo>();
                await Actor.OpenChannel(channel,  (a,c) => a.DownloadBarSeriesToStorage(c, symbol, timeFrame, priceType, from, to));
                return channel;
            }

            public async Task<Channel<SliceInfo>> DownloadTickSeriesToStorage(string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
            {
                var channel = Channel.NewOutput<SliceInfo>();
                await Actor.OpenChannel(channel, (a, c) => a.DownloadTickSeriesToStorage(c, symbol, timeFrame, from, to));
                return channel;
            }
        }

        public FeedCache Cache => _diskCache;

        private async Task Start(IFeedServerApi feed, string server, string login)
        {
            _feedProxy = feed;

            var onlineFolder = _dataFolder;
            if (_folderOptions == FeedHistoryFolderOptions.ServerHierarchy || _folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(server));
            if (_folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(login));

            await Task.Factory.StartNew(() => _diskCache.Start(onlineFolder));
        }

        private async Task Stop()
        {
            try
            {
                _feedProxy = null;
                await Task.Factory.StartNew(() => _diskCache.Stop());
            }
            catch (Exception ex)
            {
                logger.Error("Init ERROR " + ex.ToString());
            }
        }

        private Task<Tuple<DateTime, DateTime>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
        {
            return _feedProxy.GetAvailableRange(symbol, priceType, timeFrame);
        }

        private Task<BarEntity[]> GetBarPage(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int count)
        {
            return _feedProxy.DownloadBarPage(symbol, startTime, count, priceType, timeFrame);
        }

        public async Task<List<BarEntity>> GetBarList(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime from, DateTime to)
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

        private void DownloadBarSeriesToStorage(Channel<SliceInfo> stream, string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            GetSeriesData(stream, symbol, timeFrame, priceType, from, to, GetCacheInfo, DownloadBarsInternal);
        }

        private void DownloadTickSeriesToStorage(Channel<SliceInfo> stream, string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
        {
            GetSeriesData(stream, symbol, timeFrame, null, from, to, GetCacheInfo, DownloadTicksInternal);
        }

        private IEnumerable<SliceInfo> GetCacheInfo(FeedCacheKey key, DateTime from, DateTime to)
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

            logger.Debug("Start downloading bars (" + from + " - " + to + ")");

            _feedProxy.DownloadBars(CreateBlocingChannel(inputStream), key.Symbol, from, to, key.PriceType.Value, key.Frame);

            var i = from;
            while (await inputStream.ReadNext())
            {
                if (barSlicer.Write(inputStream.Current))
                {
                    var slice = barSlicer.CompleteSlice(false);

                    logger.Debug("downloaded slice {0} - {1}", slice.From, slice.To);
                    //var slice = new BarStreamSlice(i, sliceTo, bars);
                    Cache.Put(key, slice.From, slice.To, slice.Items);
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
                Cache.Put(key, lastSlice.From, lastSlice.To, lastSlice.Items);
                logger.Debug("downloaded slice {0} - {1}", lastSlice.From, lastSlice.To);
                if (!await outputAction(lastSlice))
                {
                    logger.Debug("Downloading canceled!");
                    throw new TaskCanceledException();
                }
                i = lastSlice.To;
            }

            return i;
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

            logger.Debug("Start downloading quotes (" + from + " - " + to + ")");

            _feedProxy.DownloadQuotes(CreateBlocingChannel(inputStream), key.Symbol, from, to, level2);

            var i = from;
            while (await inputStream.ReadNext())
            {
                if (quoteSlicer.Write(inputStream.Current))
                {
                    var slice = quoteSlicer.CompleteSlice(false);

                    logger.Debug("downloaded slice {0} - {1}", slice.From, slice.To);
                    //var slice = new BarStreamSlice(i, sliceTo, bars);
                    Cache.Put(key, slice.From, slice.To, slice.Items);
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
                Cache.Put(key, lastSlice.From, lastSlice.To, lastSlice.Items);
                logger.Debug("downloaded slice {0} - {1}", lastSlice.From, lastSlice.To);
                if (!await outputAction(lastSlice))
                {
                    logger.Debug("Downloading canceled!");
                    throw new TaskCanceledException();
                }
                i = lastSlice.To;
            }

            return i;
        }

        private async void GetSeriesData<TOut>(Channel<TOut> buffer,
            string symbol, TimeFrames timeFrame, BarPriceType? priceType, DateTime from, DateTime to,
            Func<FeedCacheKey, DateTime, DateTime, IEnumerable<TOut>> cacheProvider,
            Func<Channel<TOut>, FeedCacheKey, DateTime, DateTime, Task<DateTime>> download)
            where TOut : SliceInfo
        {
            try
            {
                var key = new FeedCacheKey(symbol, timeFrame, priceType);
                var i = from;
                foreach (var cacheItem in cacheProvider(key, from, to))
                {
                    if (cacheItem.From == i)
                    {
                        if (!await buffer.Write(cacheItem))
                            return;
                        i = cacheItem.To;
                    }
                    else
                        i = await download(buffer, key, i, cacheItem.From);
                }

                if (i < to)
                    i = await download(buffer, key, i, to);

                await buffer.Close();
            }
            catch (Exception ex)
            {
                await buffer.Close(ex);
            }
        }
    }

    public enum FeedHistoryFolderOptions
    {
        NoHierarchy, // places history right into specified folder
        ServerHierarchy, // creates subfolder for server
        ServerClientHierarchy // creates subfolder for server and nested subfolder for client.
    }
}
