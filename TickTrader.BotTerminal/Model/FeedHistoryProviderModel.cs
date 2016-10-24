using Machinarium.State;
using NLog;
using SoftFX.Extended;
using SoftFX.Extended.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class FeedHistoryProviderModel
    {
        private Logger logger;
        private ConnectionModel connection;
        private DataFeedStorage fdkStorage;
        private BufferBlock<Task> requestQueue = new BufferBlock<Task>();
        private ActionBlock<Task> requestProcessor;
        private IDisposable pipeLink;

        public FeedHistoryProviderModel(ConnectionModel connection)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.connection = connection;

            connection.SysInitalizing += Connection_Initalizing;
            connection.SysDeinitalizing += Connection_Deinitalizing;

            requestProcessor = new ActionBlock<Task>(t => t.RunSynchronously(), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });
        }

        private Task Connection_Initalizing(object sender, CancellationToken cancelToken)
        {
            return Init();
        }

        private Task Connection_Deinitalizing(object sender, CancellationToken cancelToken)
        {
            return Deinit();
        }

        private async Task Init()
        {
            fdkStorage = await Task.Factory.StartNew(
                () => new DataFeedStorage(EnvService.Instance.FeedHistoryCacheFolder,
                    StorageProvider.Ntfs, 2, connection.FeedProxy, false, false));
            pipeLink = requestQueue.LinkTo(requestProcessor); // start processing
        }

        private async Task Deinit()
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

        public Task<Quote[]> GetTicks(string symbol, DateTime startTime, DateTime endTime, int depth)
        {
            return Enqueue(() => fdkStorage.Online.GetQuotes(symbol, startTime, endTime, depth));
        }

        public Task<Bar[]> GetBars(string symbol, PriceType priceType, BarPeriod period, DateTime startTime, DateTime endTime)
        {
            return Enqueue(() => fdkStorage.Online.GetBars(symbol, priceType, period, startTime, endTime));
        }

        public Task<Bar[]> GetBars(string symbol, PriceType priceType, BarPeriod period, DateTime startTime, int count)
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
}
