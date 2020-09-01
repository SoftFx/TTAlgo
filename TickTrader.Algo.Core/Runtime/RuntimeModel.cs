using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class RuntimeModel
    {
        private readonly AlgoPluginRef _pluginRef;
        private readonly ISyncContext _syncContext;
        private readonly IRuntimeHostProxy _runtimeHost;


        public string Id { get; }

        public IAccountInfoProvider AccInfoProvider { get; set; }

        public ITradeHistoryProvider TradeHistoryProvider { get; set; }

        public IPluginMetadata Metadata { get; set; }

        public IFeedProvider Feed { get; set; }

        public IFeedHistoryProvider FeedHistory { get; set; }

        public ITradeExecutor TradeExecutor { get; set; }

        public RuntimeConfig Config { get; } = new RuntimeConfig();

        public event Action<UnitLogRecord> LogUpdated;
        public event Action<DataSeriesUpdate> OutputUpdate;
        public event Action<Exception> ErrorOccurred;
        public event Action<RuntimeModel> Stopped;


        internal RuntimeModel(string id, AlgoPluginRef pluginRef, ISyncContext updatesSync)
        {
            Id = id;
            _pluginRef = pluginRef;
            _syncContext = updatesSync;

            _runtimeHost = RuntimeHost.Create(_pluginRef.IsIsolated);
        }


        public Task Start(string address, int port)
        {
            return _runtimeHost.Start(address, port, Id);
        }

        public Task Stop()
        {
            return _runtimeHost.Stop();
        }


        internal void OnLogUpdated(UnitLogRecord record)
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
            //if (update.SeriesType == DataSeriesUpdate.Types.Type.SymbolRate)
            //{
            //    var bar = update.Value.Unpack<BarData>();
            //    ChartBarUpdated?.Invoke(bar, update.SeriesId, update.UpdateAction);
            //}
            //else if (update.SeriesType == DataSeriesUpdate.Types.Type.NamedStream)
            //{
            //    var bar = update.Value.Unpack<BarData>();
            //    if (update.SeriesId == BacktesterCollector.EquityStreamName)
            //        EquityUpdated?.Invoke(bar, update.UpdateAction);
            //    else if (update.SeriesId == BacktesterCollector.MarginStreamName)
            //        MarginUpdated?.Invoke(bar, update.UpdateAction);
            //}
            //else if (update.SeriesType == DataSeriesUpdate.Types.Type.Output)
            if (update.SeriesType == DataSeriesUpdate.Types.Type.Output)
                OutputUpdate?.Invoke(update);
        }
    }
}
