using Machinarium.State;
using SoftFX.Extended;
using SoftFX.Extended.Storage;
using System;
using System.Collections.Generic;
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

        public Task<List<BarEntity>> GetBars(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int count)
        {
            return Task.Factory.StartNew(() =>
            {
                var result = connection.FeedProxy.Server.GetHistoryBars(symbol, startTime, count, FdkToAlgo.Convert(priceType), FdkToAlgo.ToBarPeriod(timeFrame));
                var algoBars = FdkToAlgo.Convert(result.Bars, true);

                var toCorrected = result.To.Value;

                if (algoBars.Count > 0 && algoBars.Last().OpenTime == result.To) // hacky workaround
                {
                    var sampler = BarSampler.Get(timeFrame);
                    var barBoundaries = sampler.GetBar(result.To.Value);
                    toCorrected = barBoundaries.Close;
                }

                _onlineCache.Put(symbol, timeFrame, priceType, result.From.Value, toCorrected, algoBars.ToArray());
                return algoBars;
            });

            //return Enqueue(() => fdkStorage.Online.GetBars(symbol, priceType, period, startTime, count));
        }

        //private Task<TResult> Enqueue<TResult>(Func<TResult> handler)
        //{
        //    Task<TResult> task = new Task<TResult>(handler);
        //    requestQueue.Post(task);
        //    return task;
        //}
    }

   
    public enum FeedHistoryFolderOptions
    {
        NoHierarchy, // places history right into specified folder
        ServerHierarchy, // creates subfolder for server
        ServerClientHierarchy // creates subfolder for server and nested subfolder for client.
    }
}
