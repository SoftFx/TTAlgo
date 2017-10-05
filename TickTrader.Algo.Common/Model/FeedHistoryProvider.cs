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

        public Task<List<BarEntity>> GetBarSlice(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int count)
        {
            return Task.Factory.StartNew(() =>
            {
                var slice = DownloadBarSlice(symbol, timeFrame, priceType, startTime, count);
                _diskCache.Put(symbol, timeFrame, priceType, slice.From, slice.To, slice.Bars.ToArray());
                return slice.Bars;
            });
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
            observer?.SetMessage("Downloading... \n");

            var watch = new Stopwatch();
            int downloadedCount = 0;

            return Task.Factory.StartNew(() =>
            {
                for (var i = from; i < to;)
                {
                    var cachedSlice = Cache.GetFirstBarRange(symbol, timeFrame, priceType.Value, i, to);
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
                        _diskCache.Put(symbol, timeFrame, priceType.Value, i, slice.To, slice.Bars.ToArray());
                        i = slice.To;
                    }

                    var msg = "Downloading... " + downloadedCount + " bars are downloaded.";
                    if (watch.ElapsedMilliseconds > 0)
                        msg += "\nBar per second: " + Math.Round( (double)(downloadedCount * 1000) / watch.ElapsedMilliseconds);
                    observer.SetMessage(msg);

                    observer?.SetProgress(i.TotalDays());

                    if (cancelToken.IsCancellationRequested)
                    {
                        observer.SetMessage("Canceled. " + downloadedCount + " bars were downloaded.");
                        return;
                    }
                }

                observer.SetMessage("Completed. " + downloadedCount + " bars were downloaded.");
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
