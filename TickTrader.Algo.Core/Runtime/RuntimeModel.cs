using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    public class RuntimeModel
    {
        public const int ShutdownTimeout = 25000;


        private readonly AlgoServer _server;
        private readonly AlgoPluginRef _pluginRef;
        private readonly IRuntimeHostProxy _runtimeHost;
        private readonly Dictionary<string, AttachedAccount> _attachedAccounts;

        private Action<RpcMessage> _onNotification;
        private IRuntimeProxy _proxy;
        private TaskCompletionSource<bool> _attachTask;


        public string Id { get; }

        internal IRuntimeProxy Proxy { get; }

        public IAccountInfoProvider AccInfoProvider { get; set; }

        public ITradeHistoryProvider TradeHistoryProvider { get; set; }

        public IPluginMetadata Metadata { get; set; }

        public IFeedProvider Feed { get; set; }

        public IFeedHistoryProvider FeedHistory { get; set; }

        public ITradeExecutor TradeExecutor { get; set; }

        public ExecutorConfig Config { get; } = new ExecutorConfig();

        public event Action<UnitLogRecord> LogUpdated;
        public event Action<DataSeriesUpdate> OutputUpdate;
        public event Action<Exception> ErrorOccurred;
        public event Action<RuntimeModel> Stopped;

        public Feed.Types.Timeframe Timeframe { get; private set; }

        public string ConnectionInfo { get; private set; }


        internal RuntimeModel(AlgoServer server, string id, AlgoPluginRef pluginRef)
        {
            _server = server;
            Id = id;
            _pluginRef = pluginRef;

            _attachedAccounts = new Dictionary<string, AttachedAccount>();

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
            await Task.WhenAny(_proxy.Stop(), Task.Delay(ShutdownTimeout));
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

            _attachTask?.TrySetResult(true);
        }

        internal void OnDetached()
        {
            _onNotification = null;
            _proxy = null;
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

        internal ExecutorModel CreateExecutor(PluginConfig config, string accountId)
        {
            return new ExecutorModel(this, config, accountId);
        }

        internal void AttachAccount(string accountId)
        {
            if (!_attachedAccounts.TryGetValue(accountId, out var account))
            {
                if (!_server.TryGetAccount(accountId, out var accountProxy))
                    throw new ArgumentException("Unknown account id");

                account = new AttachedAccount(this, accountProxy);
            }
            account.AddRef();
        }

        internal void DetachAccount(string accountId)
        {
            if (!_attachedAccounts.TryGetValue(accountId, out var account))
                    throw new ArgumentException("Unknown account id");

            var refCnt = account.RemoveRef();
            if (refCnt == 0)
                _attachedAccounts.Remove(accountId);
        }


        private class AttachedAccount
        {
            private readonly RuntimeModel _runtime;
            private readonly IAccountProxy _account;
            private readonly string _accId;
            private readonly object _lock = new object();
            private int _refCnt;


            public AttachedAccount(RuntimeModel runtime, IAccountProxy account)
            {
                _runtime = runtime;
                _account = account;
                _accId = _account.Id;
            }


            public int AddRef()
            {
                lock (_lock)
                {
                    if (_refCnt == 0)
                    {
                        _account.AccInfoProvider.OrderUpdated += OnOrderUpdated;
                        _account.AccInfoProvider.PositionUpdated += OnPositionUpdated;
                        _account.AccInfoProvider.BalanceUpdated += OnBalanceUpdated;

                        _account.Feed.RateUpdated += OnRateUpdated;
                        _account.Feed.RatesUpdated += OnRatesUpdated;
                    }
                    _refCnt++;

                    return _refCnt;
                }
            }

            public int RemoveRef()
            {
                lock(_lock)
                {
                    _refCnt--;
                    if (_refCnt == 0)
                    {
                        _account.AccInfoProvider.OrderUpdated -= OnOrderUpdated;
                        _account.AccInfoProvider.PositionUpdated -= OnPositionUpdated;
                        _account.AccInfoProvider.BalanceUpdated -= OnBalanceUpdated;

                        _account.Feed.RateUpdated -= OnRateUpdated;
                        _account.Feed.RatesUpdated -= OnRatesUpdated;
                    }

                    return _refCnt;
                }
            }


            private void OnOrderUpdated(OrderExecReport r) => _runtime._onNotification?.Invoke(RpcMessage.Notification(_accId, r));

            private void OnPositionUpdated(PositionExecReport r) => _runtime._onNotification?.Invoke(RpcMessage.Notification(_accId, r));

            private void OnBalanceUpdated(BalanceOperation r) => _runtime._onNotification?.Invoke(RpcMessage.Notification(_accId, r));

            private void OnRateUpdated(QuoteInfo r) => _runtime._onNotification?.Invoke(RpcMessage.Notification(_accId, r.GetFullQuote()));

            private void OnRatesUpdated(List<QuoteInfo> r) => _runtime._onNotification?.Invoke(RpcMessage.Notification(_accId, QuotePage.Create(r)));
        }
    }
}
