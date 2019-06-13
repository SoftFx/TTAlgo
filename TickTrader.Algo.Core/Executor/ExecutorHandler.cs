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
        
        private AlgoPluginRef _ref;
        private IUpdateWorker _worker;

        public ExecutorHandler(AlgoPluginRef pluginRef, ISynchronizationContext updatesSync)
        {
            _ref = pluginRef;
            _syncContext = updatesSync;
            Core = pluginRef.CreateExecutor();
            IsIsolated = pluginRef.IsIsolated;
        }

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
        public event Action<Exception> ErrorOccurred;

        internal void StartCollection(bool realtime)
        {
            if (realtime)
                _worker = _ref.CreateObject<RealtimeUpdateWorker>();
            else
                _worker = _ref.CreateObject<BulckUpdateWorker>();

            _worker.Start(this, Core);
        }

        internal void StopCollection()
        {
            try
            {
                _worker.Stop();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
            }
        }

        private void MarshalUpdates(IList<object> updates)
        {
            _syncContext.Invoke(() =>
            {
                for (int i = 0; i < updates.Count; i++)
                {
                    try
                    {
                        MarshalUpdate(updates[i]);
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke(ex);
                    }
                }
            });
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

        public interface IUpdateWorker
        {
            void Start(ExecutorHandler handler, PluginExecutor executor);
            void Stop();
        }

        public class BulckUpdateWorker : CrossDomainObject, IUpdateWorker
        {
            private PagedGate<object> _gate = new PagedGate<object>(300);
            private Task _gatePushTask;
            private ExecutorHandler _handler;

            public void Start(ExecutorHandler handler, PluginExecutor executor)
            {
                _handler = handler;
                executor.OnUpdate = EnqueueUpdate;
                executor.Stopped += Executor_Stopped;

                _gatePushTask = Task.Factory.StartNew(PushUpdates);
            }

            public void Stop()
            {
                //_gate.Close();
                _gatePushTask.Wait();
            }

            private void Executor_Stopped()
            {
                _gate.Complete();
            }

            private void EnqueueUpdate(object update)
            {
                _gate.Write(update);
            }

            private void PushUpdates()
            {
                foreach (var page in _gate.PagedRead())
                    _handler.MarshalUpdates(page);
            }
        }

        public class RealtimeUpdateWorker : CrossDomainObject, IUpdateWorker
        {
            private ExecutorHandler _handler;
            private BufferBlock<object> _updateBuffer;
            private ActionBlock<object[]> _updateSender;
            private Task _batchJob;

            public void Start(ExecutorHandler handler, PluginExecutor executor)
            {
                _handler = handler;
                executor.OnUpdate = EnqueueUpdate;

                var bufferOptions = new DataflowBlockOptions() { BoundedCapacity = 200 };
                var senderOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 10, SingleProducerConstrained = true };

                _updateBuffer = new BufferBlock<object>(bufferOptions);
                _updateSender = new ActionBlock<object[]>(msgList => _handler.MarshalUpdates(msgList), senderOptions);

                _batchJob = _updateBuffer.BatchLinkTo(_updateSender, 50);
            }

            public void Stop()
            {
                _updateBuffer.Complete();
                _updateBuffer.Completion.Wait();
                _batchJob.Wait();
                _updateSender.Complete();
                _updateSender.Completion.Wait();
            }

            private void EnqueueUpdate(object update)
            {
                _updateBuffer.SendAsync(update).Wait();
            }
        }
    }
}
