using Machinarium.State;
using SoftFX.Extended;
using SoftFX.Extended.Storage;
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
using TickTrader.Algo.Core.Math;
using TickTrader.SeriesStorage;

namespace TickTrader.Algo.Common.Model
{
    public class FeedHistoryProviderModel
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<FeedHistoryProviderModel>();

        private ConnectionModel connection;
        //private BufferBlock<Task> requestQueue = new BufferBlock<Task>();
        //private ActionBlock<Task> requestProcessor;
        //private IDisposable pipeLink;
        private string _dataFolder;
        private string _customDataFolder;
        private FeedHistoryFolderOptions _folderOptions;
        private FeedCache _customChache;
        private FeedCache _onlineCache;

        public FeedHistoryProviderModel(ConnectionModel connection, string onlieDataFolder, string cutsomDataFolder, FeedHistoryFolderOptions folderOptions)
        {
            _dataFolder = onlieDataFolder;
            _customDataFolder = cutsomDataFolder;
            this.connection = connection;
            _folderOptions = folderOptions;

            //requestProcessor = new ActionBlock<Task>(t => t.RunSynchronously(), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });
        }

        private Task Connection_Initalizing(object sender, CancellationToken cancelToken)
        {
            return Init();
        }

        private Task Connection_Deinitalizing(object sender, CancellationToken cancelToken)
        {
            return Deinit();
        }

        public FeedCache OnlineCache => _onlineCache;
        public FeedCache CustomCache => _customChache;

        public async Task Init()
        {
            var onlineFolder = _dataFolder;
            if (_folderOptions == FeedHistoryFolderOptions.ServerHierarchy || _folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(connection.CurrentServer));
            if (_folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(connection.CurrentLogin));

            _onlineCache = new FeedCache(onlineFolder);
            _customChache = new FeedCache(_customDataFolder);

            await Task.Factory.StartNew(() =>
            {
                _onlineCache.Start();
                _customChache.Start();
            });
        }

        public async Task Deinit()
        {
            try
            {
                //pipeLink.Dispose(); // deattach buffer from the processor

                await Task.Factory.StartNew(() =>
                {
                    _onlineCache.Stop();
                    _customChache.Stop();

                    //fdkStorage.Bind(null);
                    //fdkStorage.Dispose();
                });
            }
            catch (Exception ex)
            {
                logger.Error("Init ERROR " + ex.ToString());
            }
        }

        //public IEnumerable<QuoteEntity> IterateTicks(string symbol, DateTime startTime, DateTime endTime, int depth)
        //{
        //    throw new NotImplementedException();

        //    //return Enqueue(() => fdkStorage.Online.GetQuotes(symbol, startTime, endTime, depth));
        //}

        //public IEnumerable<BarEntity> IterateBars(string symbol, BarPriceType priceType, TimeFrames period, DateTime startTime, DateTime endTime)
        //{
        //    throw new NotImplementedException();

        //    //return Enqueue(() => fdkStorage.Online.GetBars(symbol, priceType, period, startTime, endTime));
        //}

        public IEnumerable<Slice<DateTime, BarEntity>> ReadCache(FeedCacheKey key, bool custom, DateTime from, DateTime to)
        {
            var cache = custom ? _customChache : _onlineCache;
            return cache.IterateBarCache(key, from, to);
        }

        public Task<Tuple<DateTime, DateTime>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
        {
            return Task.Factory.StartNew(() =>
            {
                if (timeFrame != TimeFrames.Ticks)
                {
                    var result = connection.FeedProxy.Server.GetHistoryBars(symbol, DateTime.Now, 1, FdkToAlgo.Convert(priceType), FdkToAlgo.ToBarPeriod(timeFrame));

                    return new Tuple<DateTime, DateTime>(result.FromAll, result.ToAll);
                }
                else
                    throw new Exception("Ticks is not supported!");
            });
        }

        public Task<Tuple<DateTime, DateTime>> GetCachedRange(FeedCacheKey key, bool custom)
        {
            return Task.Factory.StartNew(() =>
            {
                var cache = custom ? _customChache : _onlineCache;
                bool hasValues = false;
                var min = DateTime.MinValue;
                var max = DateTime.MaxValue;

                foreach (var r in cache.IterateCacheKeys(key))
                {
                    if (!hasValues)
                    {
                        min = r.From;
                        hasValues = true;
                    }

                    max = r.To;
                }

                return hasValues ? new Tuple<DateTime, DateTime>(min, max) : null;
            });
        }

        public Task<List<BarEntity>> GetBarSlice(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int count)
        {
            return Task.Factory.StartNew(() =>
            {
                var slice = DownloadBarSlice(symbol, timeFrame, priceType, startTime, count);
                _onlineCache.Put(symbol, timeFrame, priceType, slice.From, slice.To, slice.Bars.ToArray());
                return slice.Bars;
            });

            //return Enqueue(() => fdkStorage.Online.GetBars(symbol, priceType, period, startTime, count));
        }

        public Task Downlaod(string symbol, TimeFrames timeFrame, BarPriceType? priceType, DateTime from, DateTime to)
        {
            return Downlaod(symbol, timeFrame, priceType, from, to, CancellationToken.None, null);
        }

        public Task Downlaod(string symbol, TimeFrames timeFrame, BarPriceType? priceType, DateTime from, DateTime to,
            CancellationToken cancelToken, IActionObserver observer = null)
        {
            const int chunkSize = 12000;

            observer?.StartProgress(from.TotalDays(), to.TotalDays());

            var watch = new Stopwatch();
            int downloadedCount = 0;

            return Task.Factory.StartNew(() =>
            {
                for (var i = from; i < to;)
                {
                    var cachedSlice = OnlineCache.GetFirstBarRange(symbol, timeFrame, priceType.Value, i, to);
                    var cachedStart = cachedSlice?.From ?? DateTime.MaxValue;
                    var cachedEnd = cachedSlice?.To ?? DateTime.MaxValue;

                    if (cachedStart <= i)
                    {
                        // skip
                        i = cachedEnd;
                    }
                    else
                    {
                        // download
                        watch.Start();
                        var slice = DownloadBarSlice(symbol, timeFrame, priceType.Value, i, chunkSize);
                        downloadedCount += slice?.Bars.Count ?? 0;
                        watch.Stop();
                        _onlineCache.Put(symbol, timeFrame, priceType.Value, i, slice.To, slice.Bars.ToArray());
                        i = slice.To;
                    }

                    observer.SetMessage(0, "Downloading... " +  downloadedCount + " bars are downloaded.");

                    if (watch.ElapsedMilliseconds > 0)
                        observer.SetMessage(1, "Bar per second: " + Math.Round( (double)(downloadedCount * 1000) / watch.ElapsedMilliseconds));

                    observer?.SetProgress(i.TotalDays());

                    if (cancelToken.IsCancellationRequested)
                    {
                        observer.SetMessage(0, "Canceled. " + downloadedCount + " bars were downloaded.");
                        return;
                    }
                }

                observer.SetMessage(0, "Completed. " + downloadedCount + " bars were downloaded.");
            });
        }

        private BarSlice DownloadBarSlice(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime startTime, int count)
        {
            var result = connection.FeedProxy.Server.GetHistoryBars(symbol, startTime, count, FdkToAlgo.Convert(priceType), FdkToAlgo.ToBarPeriod(timeFrame));
            var algoBars = FdkToAlgo.Convert(result.Bars, count < 0);

            var toCorrected = result.To.Value;

            if (algoBars.Count > 0 && algoBars.Last().OpenTime == result.To) // hacky workaround
            {
                var sampler = BarSampler.Get(timeFrame);
                var barBoundaries = sampler.GetBar(result.To.Value);
                toCorrected = barBoundaries.Close;
            }

            return new BarSlice { Bars = algoBars, From = result.From.Value, To = toCorrected };
        }

        private class BarSlice
        {
            public DateTime From { get; set; }
            public DateTime To { get; set; }
            public List<BarEntity> Bars { get; set; }
        }
    }

   
    public enum FeedHistoryFolderOptions
    {
        NoHierarchy, // places history right into specified folder
        ServerHierarchy, // creates subfolder for server
        ServerClientHierarchy // creates subfolder for server and nested subfolder for client.
    }
}
