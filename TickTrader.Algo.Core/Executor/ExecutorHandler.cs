using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core
{
    public class ExecutorHandler : CrossDomainObject
    {
        private ISynchronizationContext _syncContext;
        private BufferBlock<object> _updateBuffer;
        private ActionBlock<object[]> _updateSender;
        private Task _batchLinkTask;

        public ExecutorHandler(AlgoPluginRef pluginRef, ISynchronizationContext updatesSync)
        {
            _syncContext = updatesSync;
            //_syncContext = SynchronizationContext.Current
            //    ?? throw new Exception("No synchronization context! ExecutorHandler can only be crteater on UI thread or inside an Actor!");

            Core = pluginRef.CreateExecutor();
            IsIsolated = pluginRef.IsIsolated;

            Core.OnUpdate = EnqueueUpdate;
        }

        public void Start()
        {
            StartCollection();
        }

        public void Stop()
        {
            StopCollection().Wait();
        }

        //public async Task Stop()
        //{
        //    await Task.Factory.StartNew(() => Core.Stop());
        //    await StopCollection();
        //}

        // TO DO: Direct access to Core should be removed in future refactoring!
        public PluginExecutor Core { get; }
        public bool IsIsolated { get; }

        public event Action<BotLogRecord> LogUpdated;
        public event Action<TesterTradeTransaction> TradesUpdated;
        public event Action<TradeReportEntity> TradeHistoryUpdated;
        public event Action<RateUpdate> SymbolRateUpdated;
        public event Action<BarEntity, string, SeriesUpdateActions> ChartBarUpdated;
        public event Action<BarEntity, SeriesUpdateActions> EquityUpdated;
        public event Action<BarEntity, SeriesUpdateActions> MarginUpdated;
        public event Action<IDataSeriesUpdate> OutputUpdate;

        private void StartCollection()
        {
            var bufferOptions = new DataflowBlockOptions() { BoundedCapacity = 30 };
            var senderOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 30 };

            _updateBuffer = new BufferBlock<object>(bufferOptions);
            _updateSender = new ActionBlock<object[]>(msgList => _syncContext.Invoke(() => MarshalUpdates(msgList)), senderOptions);

            _batchLinkTask = _updateBuffer.BatchLinkTo(_updateSender, 30);
        }

        private async Task StopCollection()
        {
            try
            {
                _updateBuffer.Complete();
                await _updateBuffer.Completion;
                await _batchLinkTask;
                _updateSender.Complete();
                await _updateSender.Completion;
            }
            catch (Exception ex)
            {
                //_context.OnInternalException(ex);
            }
        }

        private void EnqueueUpdate(object update)
        {
            _updateBuffer.SendAsync(update).Wait();
        }

        private void MarshalUpdates(object[] updates)
        {
            foreach (var update in updates)
            {
                try
                {
                    MarshalUpdate(update);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void MarshalUpdate(object update)
        {
            if (update is BotLogRecord)
                LogUpdated?.Invoke((BotLogRecord)update);
            else if (update is TradeReportEntity)
                TradeHistoryUpdated?.Invoke((TradeReportEntity)update);
            else if (update is TesterTradeTransaction)
                TradesUpdated?.Invoke((TesterTradeTransaction)update);
            else if (update is RateUpdate)
                SymbolRateUpdated?.Invoke((RateUpdate)update);
            else if (update is IDataSeriesUpdate)
            {
                var seriesUpdate = (IDataSeriesUpdate)update;
                if (seriesUpdate.SeriesType == DataSeriesTypes.SymbolRate)
                {
                    var barUpdate = (DataSeriesUpdate<BarEntity>)update;
                    ChartBarUpdated?.Invoke(barUpdate.Value, barUpdate.SeriesId, barUpdate.Action);
                }
                else if (seriesUpdate.SeriesType == DataSeriesTypes.NamedStream)
                {
                    var barUpdate = (DataSeriesUpdate<BarEntity>)update;
                    if (barUpdate.SeriesId == BacktesterCollector.EquityStreamName)
                        EquityUpdated?.Invoke(barUpdate.Value, barUpdate.Action);
                    else if (barUpdate.SeriesId == BacktesterCollector.MarginStreamName)
                        MarginUpdated?.Invoke(barUpdate.Value, barUpdate.Action);
                }
                else if (seriesUpdate.SeriesType == DataSeriesTypes.Output)
                    OutputUpdate?.Invoke(seriesUpdate);
            }
        }
    }
}
