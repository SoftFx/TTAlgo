using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    public class RuntimeModel
    {
        public const int ShutdownTimeout = 25000;


        private readonly AlgoServer _server;
        private readonly string _packagePath;
        private readonly IRuntimeHostProxy _runtimeHost;
        private readonly Dictionary<string, AttachedAccount> _attachedAccounts;

        private Action<RpcMessage> _onNotification;
        private IRuntimeProxy _proxy;
        private TaskCompletionSource<bool> _attachTask, _launchTask;


        public string Id { get; }

        internal IRuntimeProxy Proxy => _proxy;

        public RuntimeConfig Config { get; } = new RuntimeConfig();


        internal RuntimeModel(AlgoServer server, string id, string packageId, string packagePath)
        {
            _server = server;
            Id = id;
            _packagePath = packagePath;

            Config.PackagePath = packagePath;
            Config.PackageId = packageId;
            _attachedAccounts = new Dictionary<string, AttachedAccount>();

            _launchTask = new TaskCompletionSource<bool>();

            _runtimeHost = RuntimeHost.Create(true);// _pluginRef.IsIsolated);
        }


        public async Task Start(string address, int port)
        {
            _attachTask = new TaskCompletionSource<bool>();
            await _runtimeHost.Start(address, port, Id);
            await _attachTask.Task;
            await _proxy.Launch();
            _launchTask.TrySetResult(true);
        }

        public async Task Stop()
        {
            await Task.WhenAny(_proxy.Stop(), Task.Delay(ShutdownTimeout));
            OnDetached();
            await _runtimeHost.Stop();
        }

        public Task WaitForLaunch()
        {
            return _launchTask.Task;
        }

        public Task<PackageInfo> GetPackageInfo()
        {
            return _proxy.GetPackageInfo();
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
            return _packagePath;
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
                _attachedAccounts.Add(accountId, account);
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

        internal void OnExecutorStopped(string executorId)
        {
            _server.OnExecutorStopped(executorId);
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
