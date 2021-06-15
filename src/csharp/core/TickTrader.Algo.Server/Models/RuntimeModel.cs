using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Runtime;

namespace TickTrader.Algo.Server
{
    public class RuntimeModel
    {
        public const int ShutdownTimeout = 25000;


        private readonly IAlgoLogger _logger;
        private readonly AlgoServer _server;
        private readonly AlgoPackageRef _pkgRef;
        private readonly IRuntimeHostProxy _runtimeHost;
        private readonly Dictionary<string, AttachedAccount> _attachedAccounts;

        private Action<RpcMessage> _onNotification;
        private IRuntimeProxy _proxy;
        private RpcSession _session;
        private TaskCompletionSource<bool> _attachTask, _launchTask;
        private int _startedExecutorsCnt;
        private bool _shutdownWhenIdle;


        public string Id { get; }

        internal IRuntimeProxy Proxy => _proxy;

        public RuntimeConfig Config { get; } = new RuntimeConfig();


        internal RuntimeModel(AlgoServer server, string id, AlgoPackageRef pkgRef)
        {
            _server = server;
            Id = id;
            _pkgRef = pkgRef;

            _logger = AlgoLoggerFactory.GetLogger($"{nameof(RuntimeModel)}({id})");

            Config.PackagePath = pkgRef.Identity.FilePath;
            Config.PackageId = pkgRef.PackageId;
            _attachedAccounts = new Dictionary<string, AttachedAccount>();

            _launchTask = new TaskCompletionSource<bool>();

            _runtimeHost = RuntimeHost.Create(true);
        }


        public async Task Start(string address, int port)
        {
            _logger.Debug("Starting...");

            try
            {
                _attachTask = new TaskCompletionSource<bool>();
                await _runtimeHost.Start(address, port, Id);
                await _attachTask.Task;
                await _proxy.Launch();
                _launchTask.TrySetResult(true);

                _logger.Debug("Started");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start");
                _server.PackageStorage.ReleasePackageRef(_pkgRef);
            }
        }

        public async Task Stop(string reason)
        {
            _logger.Debug($"Stopping. Reason: {reason}");

            try
            {
                await _launchTask.Task;
                var t1 = _proxy.Stop();
                var t2 = Task.Delay(ShutdownTimeout);
                var t = await Task.WhenAny(t1, t2);
                if (t == t2)
                {
                    _logger.Error("No response for stop request. Considering process hanged");
                }
                await _session.Disconnect(reason);
                OnDetached();
                await _runtimeHost.Stop();

                _logger.Debug("Stopped");

                _server.OnRuntimeStopped(Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop");
            }
            finally
            {
                _server.PackageStorage.ReleasePackageRef(_pkgRef);
            }
        }

        public Task WaitForLaunch()
        {
            return _launchTask.Task;
        }

        public Task<PackageInfo> GetPackageInfo()
        {
            return _proxy.GetPackageInfo();
        }

        public void SetShutdown()
        {
            _shutdownWhenIdle = true;
            _logger.Debug("Marked for shutdown");
            if (_startedExecutorsCnt == 0)
                Task.Factory.StartNew(ShutdownInternal);
        }


        internal void OnAttached(RpcSession session)
        {
            _onNotification = session.Tell;
            _proxy = new RuntimeProxy(session);
            _session = session;

            _attachTask?.TrySetResult(true);
        }

        internal void OnDetached()
        {
            _onNotification = null;
            _proxy = null;
            _session = null;
        }

        internal string GetPackagePath()
        {
            return _pkgRef.Identity.FilePath;
        }

        internal ExecutorModel CreateExecutor(PluginConfig config, string accountId)
        {
            return new ExecutorModel(this, config, accountId);
        }

        internal void AttachAccount(string accountId)
        {
            _logger.Debug($"Attaching account {accountId}...");

            try
            {
                if (!_attachedAccounts.TryGetValue(accountId, out var account))
                {
                    if (!_server.TryGetAccount(accountId, out var accountProxy))
                        throw new ArgumentException("Unknown account id");

                    account = new AttachedAccount(this, accountProxy);
                    _attachedAccounts.Add(accountId, account);
                }
                var refCnt = account.AddRef();
                _logger.Debug($"Attached account {accountId}. Have {refCnt} active refs");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to attach account {accountId}");
                throw;
            }
        }

        internal void DetachAccount(string accountId)
        {
            _logger.Debug($"Detaching account {accountId}...");

            try
            {
                if (!_attachedAccounts.TryGetValue(accountId, out var account))
                    throw new ArgumentException("Unknown account id");

                var refCnt = account.RemoveRef();
                _logger.Debug($"Detached account {accountId}. Have {refCnt} active refs");
                if (refCnt == 0)
                    _attachedAccounts.Remove(accountId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to detach account {accountId}");
                throw;
            }
        }

        internal void OnExecutorStarted(string executorId)
        {
            var cnt = Interlocked.Increment(ref _startedExecutorsCnt);

            _logger.Debug($"Executor {executorId} started. Have {cnt} active executors");
        }

        internal void OnExecutorStopped(string executorId)
        {
            var cnt = Interlocked.Decrement(ref _startedExecutorsCnt);

            _logger.Debug($"Executor {executorId} stopped. Have {cnt} active executors");

            _server.OnExecutorStopped(executorId);

            if (cnt == 0 && _shutdownWhenIdle)
                Task.Factory.StartNew(ShutdownInternal);
        }


        private async Task ShutdownInternal()
        {
            _logger.Debug($"Shutdown initiated");
            try
            {
                await Stop("Idle shutdown");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to shutdown");
            }
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
