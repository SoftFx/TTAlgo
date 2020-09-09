using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    public class RuntimeModel
    {
        private readonly AlgoPluginRef _pluginRef;
        private readonly ISyncContext _syncContext;
        private readonly IRuntimeHostProxy _runtimeHost;

        private Action<RpcMessage> _onNotification;
        private IRuntimeProxy _proxy;
        private TaskCompletionSource<bool> _attachTask;


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

        public Feed.Types.Timeframe Timeframe { get; private set; }

        public string ConnectionInfo { get; private set; }


        internal RuntimeModel(string id, AlgoPluginRef pluginRef, ISyncContext updatesSync)
        {
            Id = id;
            _pluginRef = pluginRef;
            _syncContext = updatesSync;

            _runtimeHost = RuntimeHost.Create(true);// _pluginRef.IsIsolated);
        }


        public void SetConfig(PluginConfig config)
        {
            Timeframe = config.Timeframe;
            Config.PluginConfig = Any.Pack(config);
        }

        public async Task Start(string address, int port)
        {
            _attachTask = new TaskCompletionSource<bool>();
            await _runtimeHost.Start(address, port, Id);
            await _attachTask.Task;
            await _proxy.Launch();
        }

        public async Task Stop()
        {
            await _proxy.Stop();
            OnDetached();
            await _runtimeHost.Stop();
        }

        public void Abort()
        {

        }

        public void NotifyDisconnectNotification()
        {
            _onNotification?.Invoke(RpcMessage.Notification(new AccountDisconnectNotification()));
        }

        public void NotifyReconnectNotification()
        {
            _onNotification?.Invoke(RpcMessage.Notification(new AccountReconnectNotification()));
        }

        public void SetConnectionInfo(string connectionInfo)
        {
            ConnectionInfo = connectionInfo;
        }


        internal void OnAttached(Action<RpcMessage> onNotification, IRuntimeProxy proxy)
        {
            _onNotification = onNotification;
            _proxy = proxy;

            AccInfoProvider.OrderUpdated += OnOrderUpdated;
            AccInfoProvider.PositionUpdated += OnPositionUpdated;
            AccInfoProvider.BalanceUpdated += OnBalanceUpdated;

            Feed.RateUpdated += OnRateUpdated;
            Feed.RatesUpdated += OnRatesUpdated;

            _attachTask?.TrySetResult(true);
        }

        internal void OnDetached()
        {
            _onNotification = null;
            _proxy = null;

            AccInfoProvider.OrderUpdated -= OnOrderUpdated;
            AccInfoProvider.PositionUpdated -= OnPositionUpdated;
            AccInfoProvider.BalanceUpdated -= OnBalanceUpdated;

            Feed.RateUpdated -= OnRateUpdated;
            Feed.RatesUpdated -= OnRatesUpdated;
        }

        internal string GetPackagePath()
        {
            return _pluginRef.PackagePath;
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


        private void OnOrderUpdated(OrderExecReport r) => _onNotification?.Invoke(RpcMessage.Notification(r));

        private void OnPositionUpdated(PositionExecReport r) => _onNotification?.Invoke(RpcMessage.Notification(r));

        private void OnBalanceUpdated(BalanceOperation r) => _onNotification?.Invoke(RpcMessage.Notification(r));

        private void OnRateUpdated(QuoteInfo r) => _onNotification?.Invoke(RpcMessage.Notification(r.GetFullQuote()));

        private void OnRatesUpdated(List<QuoteInfo> r) => _onNotification?.Invoke(RpcMessage.Notification(QuotePage.Create(r)));
    }
}
