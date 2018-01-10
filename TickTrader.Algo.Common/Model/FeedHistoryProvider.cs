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
    public class FeedHistoryProviderModel
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<FeedHistoryProviderModel>();

        private const int SliceMaxSize = 8000;

        private ConnectionModel connection;
        private string _dataFolder;
        private FeedHistoryFolderOptions _folderOptions;
        private FeedCache _diskCache = new FeedCache();

        public FeedHistoryProviderModel(ConnectionModel connection, string onlieDataFolder, FeedHistoryFolderOptions folderOptions)
        {
            _dataFolder = onlieDataFolder;
            this.connection = connection;
            _folderOptions = folderOptions;
        }

        private Task Connection_Initalizing(object sender, CancellationToken cancelToken)
        {
            return Init();
        }

        private Task Connection_Deinitalizing(object sender, CancellationToken cancelToken)
        {
            return Deinit();
        }

        #region Public Interface

        public FeedCache Cache => _diskCache;

        public async Task Init()
        {
            var onlineFolder = _dataFolder;
            if (_folderOptions == FeedHistoryFolderOptions.ServerHierarchy || _folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(connection.CurrentServer));
            if (_folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(connection.CurrentLogin));

            await Task.Factory.StartNew(() => _diskCache.Start(onlineFolder));
        }

        public async Task Deinit()
        {
            try
            {
                await Task.Factory.StartNew(() => _diskCache.Stop());
            }
            catch (Exception ex)
            {
                logger.Error("Init ERROR " + ex.ToString());
            }
        }

        public Task<Tuple<DateTime, DateTime>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
        {
            return connection.FeedProxy.GetAvailableRange(symbol, priceType, timeFrame);
        }

        public Task<BarEntity[]> GetBarPage(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int count)
        {
            return connection.FeedProxy.DownloadBarPage(symbol, startTime, count, priceType, timeFrame);
        }

        //public IAsyncEnumerator<BarStreamSlice> GetBars(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        //{
        //    var buffer = new AsyncBuffer<BarStreamSlice>(2);
        //    EnumerateBarsInternal(buffer, symbol, timeFrame, priceType, from, to);
        //    return buffer;
        //}

        public IAsyncEnumerator<SliceInfo> DownloadBarSeriesToStorage(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            var buffer = new AsyncBuffer<SliceInfo>();
            GetSeriesData(buffer, symbol, timeFrame, priceType, from, to, GetCacheInfo, DownloadBarsInternal);
            return buffer;
        }

        public IAsyncEnumerator<SliceInfo> DownloadTickSeriesToStorage(string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
        {
            var buffer = new AsyncBuffer<SliceInfo>();
            GetSeriesData(buffer, symbol, timeFrame, null, from, to, GetCacheInfo, DownloadTicksInternal);
            return buffer;
        }

        #endregion

        private IEnumerable<SliceInfo> GetCacheInfo(FeedCacheKey key, DateTime from, DateTime to)
        {
            return _diskCache.IterateCacheKeys(key, from, to).Select(s => new SliceInfo(s.From, s.To, 0));
        }

        private Task<DateTime> DownloadBarsInternal(AsyncBuffer<Slice<BarEntity>> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadBarsInternal(s => buffer.WriteAsync(s), key, from, to);
        }

        private Task<DateTime> DownloadBarsInternal(AsyncBuffer<SliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadBarsInternal(s => buffer.WriteAsync(s), key, from, to);
        }

        private async Task<DateTime> DownloadBarsInternal(Func<Slice<BarEntity>, Task<bool>> outputAction, FeedCacheKey key, DateTime from, DateTime to)
        {
            using (var barEnum = connection.FeedProxy.DownloadBars(key.Symbol, from, to, key.PriceType.Value, key.Frame))
            {
                var i = from;
                while (await barEnum.Next().ConfigureAwait(false))
                {
                    var slice = barEnum.Current;
                    //var sliceTo = bars.Last().CloseTime;
                    Debug.WriteLine("downloaded slice {0} - {1}", slice.From, slice.To);
                    //var slice = new BarStreamSlice(i, sliceTo, bars);
                    Cache.Put(key, slice.From, slice.To, slice.Items);
                    if (!await outputAction(slice).ConfigureAwait(false))
                        break;
                    i = slice.To;
                }
                return i;
            }
        }

        private Task<DateTime> DownloadTicksInternal(AsyncBuffer<Slice<QuoteEntity>> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadTicksInternal(s => buffer.WriteAsync(s), key, from, to);
        }

        private Task<DateTime> DownloadTicksInternal(AsyncBuffer<SliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to)
        {
            return DownloadTicksInternal(s => buffer.WriteAsync(s), key, from, to);
        }

        private async Task<DateTime> DownloadTicksInternal(Func<Slice<QuoteEntity>, Task<bool>> outputAction, FeedCacheKey key, DateTime from, DateTime to)
        {
            var level2 = key.Frame == TimeFrames.TicksLevel2;

            using (var barEnum = connection.FeedProxy.DownloadQuotes(key.Symbol, from, to, level2))
            {
                var i = from;
                while (await barEnum.Next().ConfigureAwait(false))
                {
                    var slice = barEnum.Current;
                    Debug.WriteLine("downloaded slice {0} - {1}", slice.From, slice.To);
                    Cache.Put(key, slice.From, slice.To, slice.Items);
                    if (!await outputAction(slice).ConfigureAwait(false))
                        break;
                    i = slice.To;
                }
                return i;
            }
        }

        private async void GetSeriesData<TOut>(AsyncBuffer<TOut> buffer,
            string symbol, TimeFrames timeFrame, BarPriceType? priceType, DateTime from, DateTime to,
            Func<FeedCacheKey, DateTime, DateTime, IEnumerable<TOut>> cacheProvider,
            Func<AsyncBuffer<TOut>, FeedCacheKey, DateTime, DateTime, Task<DateTime>> downloadProvider)
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
                        var writeFlag = await buffer.WriteAsync(cacheItem);
                        if (!writeFlag)
                            return;
                        i = cacheItem.To;
                    }
                    else
                        i = await downloadProvider(buffer, key, i, cacheItem.From);
                }

                if (i < to)
                    i = await downloadProvider(buffer, key, i, to);

                buffer.Dispose();
            }
            catch (Exception ex)
            {
                buffer.SetFailed(ex);
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
