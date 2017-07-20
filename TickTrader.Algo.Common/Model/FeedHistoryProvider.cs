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
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public abstract class FeedHistoryProviderModel
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<FeedHistoryProviderModel>();

        public static FeedHistoryProviderModel CreateDiskStorage(ConnectionModel connection, string dataFolder, FeedHistoryFolderOptions folderOptions)
        {
            return new CachingImpl(connection, dataFolder, folderOptions);
        }

        public static FeedHistoryProviderModel CreateLightProxy(ConnectionModel connection)
        {
            return new LightProxy(connection);
        }

        public abstract Task<Quote[]> GetTicks(string symbol, DateTime startTime, DateTime endTime, int depth);
        public abstract Task<Bar[]> GetBars(string symbol, PriceType priceType, BarPeriod period, DateTime startTime, DateTime endTime);
        public abstract Task<Bar[]> GetBars(string symbol, PriceType priceType, BarPeriod period, DateTime startTime, int count);
        public virtual Task Init() { return Task.FromResult(1); }
        public virtual Task Deinit() { return Task.FromResult(1); }

        private class CachingImpl : FeedHistoryProviderModel
        {
            private ConnectionModel connection;
            private DataFeedStorage fdkStorage;
            private BufferBlock<Task> requestQueue = new BufferBlock<Task>();
            private ActionBlock<Task> requestProcessor;
            private IDisposable pipeLink;
            private string _dataFolder;
            private FeedHistoryFolderOptions _folderOptions;

            public CachingImpl(ConnectionModel connection, string dataFolder, FeedHistoryFolderOptions folderOptions)
            {
                _dataFolder = dataFolder;
                this.connection = connection;
                _folderOptions = folderOptions;

                requestProcessor = new ActionBlock<Task>(t => t.RunSynchronously(), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });
            }

            public override async Task Init()
            {
                var historyFolder = _dataFolder;
                if (_folderOptions == FeedHistoryFolderOptions.ServerHierarchy || _folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                    historyFolder = Path.Combine(historyFolder, PathEscaper.Escape(connection.CurrentServer));
                if (_folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                    historyFolder = Path.Combine(historyFolder, PathEscaper.Escape(connection.CurrentLogin));

                fdkStorage = await Task.Factory.StartNew(
                    () => new DataFeedStorage(historyFolder, StorageProvider.Ntfs, 3, connection.FeedProxy, false, false));
                pipeLink = requestQueue.LinkTo(requestProcessor); // start processing
            }

            public async Task Deinit()
            {
                try
                {
                    pipeLink.Dispose(); // deattach buffer from the processor

                    await Task.Factory.StartNew(() =>
                    {
                        fdkStorage.Bind(null);
                        fdkStorage.Dispose();
                    });
                }
                catch (Exception ex)
                {
                    logger.Error("Init ERROR " + ex.ToString());
                }
            }

            public override Task<Quote[]> GetTicks(string symbol, DateTime startTime, DateTime endTime, int depth)
            {
                return Enqueue(() => fdkStorage.Online.GetQuotes(symbol, startTime, endTime, depth));
            }

            public override Task<Bar[]> GetBars(string symbol, PriceType priceType, BarPeriod period, DateTime startTime, DateTime endTime)
            {
                return Enqueue(() => fdkStorage.Online.GetBars(symbol, priceType, period, startTime, endTime));
            }

            public override Task<Bar[]> GetBars(string symbol, PriceType priceType, BarPeriod period, DateTime startTime, int count)
            {
                return Enqueue(() => fdkStorage.Online.GetBars(symbol, priceType, period, startTime, count));
            }

            private Task<TResult> Enqueue<TResult>(Func<TResult> handler)
            {
                Task<TResult> task = new Task<TResult>(handler);
                requestQueue.Post(task);
                return task;
            }
        }

        private class LightProxy : FeedHistoryProviderModel
        {
            private ActionBlock<Task> _requestProcessor;
            private ConnectionModel _connection;

            public LightProxy(ConnectionModel connection)
            {
                _connection = connection;
                _requestProcessor = new ActionBlock<Task>(t => t.RunSynchronously(), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });
            }

            public override Task<Bar[]> GetBars(string symbol, PriceType priceType, BarPeriod period, DateTime startTime, DateTime endTime)
            {
                return Enqueue(()=> _connection.FeedProxy.Server.GetBarsHistory(symbol, priceType, period, startTime, endTime).ToArray());
            }

            public override Task<Bar[]> GetBars(string symbol, PriceType priceType, BarPeriod period, DateTime startTime, int count)
            {
                return Enqueue(() => _connection.FeedProxy.Server.GetHistoryBars(symbol, startTime, count, priceType, period).Bars);
            }

            public override Task<Quote[]> GetTicks(string symbol, DateTime startTime, DateTime endTime, int depth)
            {
                throw new NotImplementedException("This feed provider does not support getting ticks!");
            }

            private Task<TResult> Enqueue<TResult>(Func<TResult> handler)
            {
                Task<TResult> task = new Task<TResult>(handler);
                _requestProcessor.Post(task);
                return task;
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
