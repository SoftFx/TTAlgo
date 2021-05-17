using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class PluginExecutor : CrossDomainObject, IDisposable
    {
        private readonly string _id;
        private ISyncContext _syncContext;

        private AlgoPluginRef _pluginRef;
        private PluginContainer _container;
        private CommonCdProxy _cProxy;
        private FeedCdProxy _fProxy;
        private TradeApiProxy _tProxy;

        internal PluginExecutor(string id, AlgoPluginRef pluginRef, ISyncContext updatesSync)
        {
            _id = id;
            _pluginRef = pluginRef;
            _syncContext = updatesSync;
            IsIsolated = pluginRef.IsIsolated;

            _container = PluginContainer.Load(_pluginRef.PackagePath, _pluginRef.IsIsolated);
            Launcher = _container.CreateObject<PluginLauncher>();
            Core = Launcher.CreateExecutor(_pluginRef.Id);

            Core.IsGlobalMarshalingEnabled = true;
            Core.IsBunchingRequired = IsIsolated || _syncContext != null;

            Core.MarshalUpdate = MarshalUpdate;
            Core.MarshalUpdates = MarshalUpdatesToContext;
        }

        internal PluginExecutorCore Core { get; private set; }
        internal PluginLauncher Launcher { get; private set; }
        public bool IsIsolated { get; }
        //public bool IsRunning { get; private set; }

        public IAccountInfoProvider AccInfoProvider { get; set; }
        public ITradeHistoryProvider TradeHistoryProvider { get; set; }
        public IPluginMetadata Metadata { get; set; }
        public IFeedProvider Feed { get; set; }
        public IFeedHistoryProvider FeedHistory { get; set; }
        public ITradeExecutor TradeExecutor { get; set; }
        public PluginExecutorConfig Config { get; } = new PluginExecutorConfig();

        public event Action<Domain.PluginLogRecord> LogUpdated;
        public event Action<TesterTradeTransaction> TradesUpdated;
        public event Action<Domain.TradeReportInfo> TradeHistoryUpdated;
        public event Action<Domain.IRateInfo> SymbolRateUpdated;
        public event Action<BarData, string, DataSeriesUpdate.Types.UpdateAction> ChartBarUpdated;
        public event Action<BarData, DataSeriesUpdate.Types.UpdateAction> EquityUpdated;
        public event Action<BarData, DataSeriesUpdate.Types.UpdateAction> MarginUpdated;
        public event Action<DataSeriesUpdate> OutputUpdate;
        public event Action<Exception> ErrorOccurred;

        public event Action<PluginExecutor> Stopped;

        internal void OnLogUpdated(Domain.PluginLogRecord record)
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
            if (update.SeriesType == Domain.DataSeriesUpdate.Types.Type.SymbolRate)
            {
                var bar = update.Value.Unpack<BarData>();
                ChartBarUpdated?.Invoke(bar, update.SeriesId, update.UpdateAction);
            }
            else if (update.SeriesType == Domain.DataSeriesUpdate.Types.Type.NamedStream)
            {
                var bar = update.Value.Unpack<BarData>();
                if (update.SeriesId == BacktesterCollector.EquityStreamName)
                    EquityUpdated?.Invoke(bar, update.UpdateAction);
                else if (update.SeriesId == BacktesterCollector.MarginStreamName)
                    MarginUpdated?.Invoke(bar, update.UpdateAction);
            }
            else if (update.SeriesType == Domain.DataSeriesUpdate.Types.Type.Output)
                OutputUpdate?.Invoke(update);
        }

        public override void Dispose()
        {
            DisposeProxies();
            _container.Dispose();
            base.Dispose();
        }

        private void DisposeProxies()
        {
            _cProxy?.Dispose();
            _fProxy?.Dispose();
            _tProxy?.Dispose();
            _cProxy = null;
            _fProxy = null;
            _tProxy = null;
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
            else if (update is Domain.IRateInfo)
                SymbolRateUpdated?.Invoke((Domain.IRateInfo)update);
            else if (update is DataSeriesUpdate)
            {
                var seriesUpdate = (DataSeriesUpdate)update;
                if (seriesUpdate.SeriesType == Domain.DataSeriesUpdate.Types.Type.SymbolRate)
                {
                    var bar = seriesUpdate.Value.Unpack<BarData>();
                    ChartBarUpdated?.Invoke(bar, seriesUpdate.SeriesId, seriesUpdate.UpdateAction);
                }
                else if (seriesUpdate.SeriesType == Domain.DataSeriesUpdate.Types.Type.NamedStream)
                {
                    var bar = seriesUpdate.Value.Unpack<BarData>();
                    if (seriesUpdate.SeriesId == BacktesterCollector.EquityStreamName)
                        EquityUpdated?.Invoke(bar, seriesUpdate.UpdateAction);
                    else if (seriesUpdate.SeriesId == BacktesterCollector.MarginStreamName)
                        MarginUpdated?.Invoke(bar, seriesUpdate.UpdateAction);
                }
                else if (seriesUpdate.SeriesType == Domain.DataSeriesUpdate.Types.Type.Output)
                    OutputUpdate?.Invoke(seriesUpdate);
            }
            //else if (update is Exception)
            //    ErrorOccurred?.Invoke((Exception)update);

            #endregion

        }
    }
}
