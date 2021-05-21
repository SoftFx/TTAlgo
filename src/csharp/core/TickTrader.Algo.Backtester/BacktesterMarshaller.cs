using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    public class BacktesterMarshaller
    {
        private ISyncContext _syncContext;

        internal BacktesterMarshaller(PluginExecutorCore executorCore, ISyncContext updatesSync)
        {
            _syncContext = updatesSync;

            Core = executorCore;

            Core.IsGlobalMarshalingEnabled = true;
            Core.IsBunchingRequired = _syncContext != null;

            Core.MarshalUpdate = MarshalUpdate;
            Core.MarshalUpdates = MarshalUpdatesToContext;
        }

        internal PluginExecutorCore Core { get; private set; }

        public event Action<PluginLogRecord> LogUpdated;
        public event Action<TesterTradeTransaction> TradesUpdated;
        public event Action<TradeReportInfo> TradeHistoryUpdated;
        public event Action<IRateInfo> SymbolRateUpdated;
        public event Action<BarData, string, DataSeriesUpdate.Types.UpdateAction> ChartBarUpdated;
        public event Action<BarData, DataSeriesUpdate.Types.UpdateAction> EquityUpdated;
        public event Action<BarData, DataSeriesUpdate.Types.UpdateAction> MarginUpdated;
        public event Action<DataSeriesUpdate> OutputUpdate;
        public event Action<Exception> ErrorOccurred;

        public event Action<BacktesterMarshaller> Stopped;

        internal void OnLogUpdated(PluginLogRecord record)
        {
            LogUpdated?.Invoke(record);
        }

        internal void OnErrorOccured(Exception ex)
        {
            ErrorOccurred?.Invoke(ex);
        }

        internal void OnStopped()
        {
            if (_syncContext != null)
                _syncContext.Invoke(() => Stopped?.Invoke(this));
            else
                Stopped?.Invoke(this);
        }

        internal void OnDataSeriesUpdate(DataSeriesUpdate update)
        {
            if (update.SeriesType == DataSeriesUpdate.Types.Type.SymbolRate)
            {
                var bar = update.Value.Unpack<BarData>();
                ChartBarUpdated?.Invoke(bar, update.SeriesId, update.UpdateAction);
            }
            else if (update.SeriesType == DataSeriesUpdate.Types.Type.NamedStream)
            {
                var bar = update.Value.Unpack<BarData>();
                if (update.SeriesId == BacktesterCollector.EquityStreamName)
                    EquityUpdated?.Invoke(bar, update.UpdateAction);
                else if (update.SeriesId == BacktesterCollector.MarginStreamName)
                    MarginUpdated?.Invoke(bar, update.UpdateAction);
            }
            else if (update.SeriesType == DataSeriesUpdate.Types.Type.Output)
                OutputUpdate?.Invoke(update);
        }


        #region Update Marshalling 

        private void MarshalUpdatesToContext(IReadOnlyList<object> updates)
        {
            if (_syncContext != null)
                _syncContext.Invoke(MarshalUpdates, updates);
            else
                MarshalUpdates(updates);
        }

        private void MarshalUpdates(IReadOnlyList<object> updates)
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
        }

        private void MarshalUpdate(object update)
        {
            //if (update is PluginLogRecord)
            //    LogUpdated?.Invoke((PluginLogRecord)update);
            //if (update is TradeReportEntity)
            //    TradeHistoryUpdated?.Invoke((TradeReportEntity)update);
            if (update is TesterTradeTransaction)
                TradesUpdated?.Invoke((TesterTradeTransaction)update);
            else if (update is IRateInfo)
                SymbolRateUpdated?.Invoke((IRateInfo)update);
            else if (update is DataSeriesUpdate)
            {
                var seriesUpdate = (DataSeriesUpdate)update;
                if (seriesUpdate.SeriesType == DataSeriesUpdate.Types.Type.SymbolRate)
                {
                    var bar = seriesUpdate.Value.Unpack<BarData>();
                    ChartBarUpdated?.Invoke(bar, seriesUpdate.SeriesId, seriesUpdate.UpdateAction);
                }
                else if (seriesUpdate.SeriesType == DataSeriesUpdate.Types.Type.NamedStream)
                {
                    var bar = seriesUpdate.Value.Unpack<BarData>();
                    if (seriesUpdate.SeriesId == BacktesterCollector.EquityStreamName)
                        EquityUpdated?.Invoke(bar, seriesUpdate.UpdateAction);
                    else if (seriesUpdate.SeriesId == BacktesterCollector.MarginStreamName)
                        MarginUpdated?.Invoke(bar, seriesUpdate.UpdateAction);
                }
                else if (seriesUpdate.SeriesType == DataSeriesUpdate.Types.Type.Output)
                    OutputUpdate?.Invoke(seriesUpdate);
            }
            //else if (update is Exception)
            //    ErrorOccurred?.Invoke((Exception)update);

            #endregion

        }
    }
}
