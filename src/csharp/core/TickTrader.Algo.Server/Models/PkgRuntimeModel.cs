﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class PkgRuntimeModel
    {
        private readonly IActorRef _impl;


        public string Id { get; }


        public PkgRuntimeModel(string id, string pkgId, string pkgRefId, AlgoServer server)
        {
            Id = id;

            _impl = ActorSystem.SpawnLocal<Impl>($"{nameof(RuntimeModel)} {id}", new InitMsg(id, pkgId, pkgRefId, server));
        }


        public Task<bool> Start() => _impl.Ask<bool>(new StartRuntimeCmd());

        public Task Stop(string reason) => _impl.Ask(new StopRuntimeCmd(reason));

        public Task<RuntimeConfig> GetConfig() => _impl.Ask<RuntimeConfig>(new RuntimeConfigRequest());


        internal void MarkForShutdown() => _impl.Tell(new MarkForShutdownCmd());

        internal Task<bool> OnConnect(RpcSession session) => _impl.Ask<bool>(new ConnectSessionCmd(session));

        internal Task<bool> AttachAccount(AttachAccountRequest request) => _impl.Ask<bool>(request);

        internal Task<bool> DetachAccount(DetachAccountRequest request) => _impl.Ask<bool>(request);

        internal Task StartExecutor(StartExecutorRequest request) => _impl.Ask(request);

        internal Task StopExecutor(StopExecutorRequest request) => _impl.Ask(request);

        internal void OnExecutorStopped(string executorId) => _impl.Tell(new ExecutorStoppedMsg(executorId));


        private class InitMsg
        {
            public string Id { get; }

            public string PkgId { get; }

            public string PkgRefId { get; }

            public AlgoServer Server { get; }

            public InitMsg(string id, string pkgId, string pkgRefId, AlgoServer server)
            {
                Id = id;
                PkgId = pkgId;
                PkgRefId = pkgRefId;
                Server = server;
            }
        }

        private class StartRuntimeCmd { }

        private class StopRuntimeCmd
        {
            public string Reason { get; }

            public StopRuntimeCmd(string reason)
            {
                Reason = reason;
            }
        }

        private class MarkForShutdownCmd { }

        private class ConnectSessionCmd
        {
            public RpcSession Session { get; }

            public ConnectSessionCmd(RpcSession session)
            {
                Session = session;
            }
        }

        private class ExecutorStoppedMsg
        {
            public string ExecutorId { get; }

            public ExecutorStoppedMsg(string executorId)
            {
                ExecutorId = executorId;
            }
        }



        private class Impl : Actor
        {
            public const int AttachTimeout = 5000;
            public const int ShutdownTimeout = 10000;


            private readonly Dictionary<string, AttachedAccount> _attachedAccounts = new Dictionary<string, AttachedAccount>();

            private IAlgoLogger _logger;
            private string _id;
            private string _pkgId;
            private string _pkgRefId;
            private AlgoServer _server;
            private IRuntimeHostProxy _runtimeHost;
            private TaskCompletionSource<bool> _startTaskSrc, _connectTaskSrc;
            private Action<RpcMessage> _onNotification;
            private IRuntimeProxy _proxy;
            private RpcSession _session;
            private int _startedExecutorsCnt;
            private bool _shutdownWhenIdle;


            public Impl()
            {
                Receive<StartRuntimeCmd, bool>(Start);
                Receive<StopRuntimeCmd>(Stop);
                Receive<MarkForShutdownCmd>(MarkForShutdown);
                Receive<ConnectSessionCmd, bool>(OnConnect);
                Receive<AttachAccountRequest, bool>(AttachAccount);
                Receive<DetachAccountRequest, bool>(DetachAccount);
                Receive<StartExecutorRequest>(StartExecutor);
                Receive<StopExecutorRequest>(StopExecutor);
                Receive<ExecutorStoppedMsg>(OnExecutorStopped);
                Receive<RuntimeConfigRequest, RuntimeConfig>(GetConfig);
            }


            protected override void ActorInit(object initMsg)
            {
                var msg = (InitMsg)initMsg;
                _id = msg.Id;
                _pkgId = msg.PkgId;
                _pkgRefId = msg.PkgRefId;
                _server = msg.Server;

                _logger = AlgoLoggerFactory.GetLogger($"{nameof(PkgRuntimeModel)}({_id})");
                _runtimeHost = RuntimeHost.Create(true);
            }


            private async Task<bool> Start(StartRuntimeCmd cmd)
            {
                if (_startTaskSrc != null)
                    return await _startTaskSrc.Task;

                _startTaskSrc = new TaskCompletionSource<bool>();
                var pkgStorage = _server.PkgStorage;

                _logger.Debug("Starting...");

                try
                {
                    var hasPkg = await pkgStorage.LockPackageRef(_pkgRefId);
                    if (!hasPkg)
                    {
                        _logger.Error($"Package ref '{_pkgRefId}' not found");
                        return false;
                    }

                    _connectTaskSrc = new TaskCompletionSource<bool>();
                    await _runtimeHost.Start(_server.Address, _server.BoundPort, _id);

                    TaskExt.Schedule(AttachTimeout, () => _connectTaskSrc?.TrySetResult(false));
                    var connected = await _connectTaskSrc.Task;
                    _connectTaskSrc = null;

                    if (connected)
                    {
                        await _proxy.Launch();

                        _startTaskSrc.TrySetResult(true);
                        _logger.Debug("Started");
                        return true;
                    }
                    else
                    {
                        _logger.Error("Failed to connect runtime host");
                        pkgStorage.ReleasePackageRef(_pkgRefId);
                        await _runtimeHost.Stop();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to start");
                }

                pkgStorage.ReleasePackageRef(_pkgRefId);
                return false;
            }

            private async Task Stop(StopRuntimeCmd cmd)
            {
                if (_startTaskSrc == null)
                    return;

                var reason = cmd.Reason;
                _logger.Debug($"Stopping. Reason: {reason}");

                try
                {
                    await _startTaskSrc.Task;
                    _startTaskSrc = null;

                    var finished = await _proxy.Stop().WaitAsync(ShutdownTimeout);
                    if (!finished)
                        _logger.Error("No response for stop request. Considering process hanged");

                    await _session.Disconnect(reason);
                    OnDetached();
                    await _runtimeHost.Stop();

                    _server.Runtimes.OnRuntimeStopped(_id);

                    _logger.Debug("Stopped");
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Failed to stop");
                }
            }

            private async Task<RuntimeConfig> GetConfig(RuntimeConfigRequest request)
            {
                var path = await _server.PkgStorage.GetPackageRefPath(_pkgRefId);
                return new RuntimeConfig { PackageId = _pkgId, PackagePath = path };
            }

            private void MarkForShutdown(MarkForShutdownCmd cmd)
            {
                _shutdownWhenIdle = true;
                _logger.Debug("Marked for shutdown");
                if (_startedExecutorsCnt == 0)
                    ShutdownInternal();
            }

            private bool OnConnect(ConnectSessionCmd cmd)
            {
                var session = cmd.Session;
                if (_connectTaskSrc == null)
                {
                    session.Disconnect("There is no pending connect");
                    return false;
                }
                if (_session != null)
                {
                    session.Disconnect("Runtime already attached");
                    return false;
                }

                _onNotification = session.Tell;
                _proxy = new RemoteRuntimeProxy(session);
                _session = session;

                _connectTaskSrc?.TrySetResult(true);

                return true;
            }

            private void OnDetached()
            {
                _onNotification = null;
                _proxy = null;
                _session = null;
            }

            private void ShutdownInternal()
            {
                _logger.Debug($"Shutdown initiated");
                Stop(new StopRuntimeCmd("Idle shutdown"))
                    .OnException(ex => _logger.Error(ex, $"Failed to shutdown"));
            }

            private async Task StartExecutor(StartExecutorRequest request)
            {
                await _proxy.StartExecutor(request.ExecutorId);
                _startedExecutorsCnt++;
                _logger.Debug($"Executor {request.ExecutorId} started. Have {_startedExecutorsCnt} active executors");
            }

            private Task StopExecutor(StopExecutorRequest request)
            {
                return _proxy.StopExecutor(request.ExecutorId);
            }

            internal void OnExecutorStopped(ExecutorStoppedMsg msg)
            {
                _startedExecutorsCnt--;
                _logger.Debug($"Executor {msg.ExecutorId} stopped. Have {_startedExecutorsCnt} active executors");

                _server.OnExecutorStopped(msg.ExecutorId);

                if (_startedExecutorsCnt == 0 && _shutdownWhenIdle)
                    ShutdownInternal();
            }

            private bool AttachAccount(AttachAccountRequest request)
            {
                var accId = request.AccountId;
                _logger.Debug($"Attaching account {accId}...");

                try
                {
                    if (!_attachedAccounts.TryGetValue(accId, out var account))
                    {
                        if (!_server.TryGetAccount(accId, out var accountProxy))
                        {
                            _logger.Error($"Can't attach account. '{accId}' doesn't exists");
                            return false;
                        }

                        account = new AttachedAccount(this, accountProxy);
                        _attachedAccounts.Add(accId, account);
                    }
                    var refCnt = account.AddRef();
                    _logger.Debug($"Attached account {accId}. Have {refCnt} active refs");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to attach account {accId}");
                }
                return false;
            }

            private bool DetachAccount(DetachAccountRequest request)
            {
                var accId = request.AccountId;
                _logger.Debug($"Detaching account {accId}...");

                try
                {
                    if (!_attachedAccounts.TryGetValue(accId, out var account))
                    {
                        _logger.Error($"Can't detach account. '{accId}' is not attached or doesn't exists");
                        return false;
                    }

                    var refCnt = account.RemoveRef();
                    _logger.Debug($"Detached account {accId}. Have {refCnt} active refs");
                    if (refCnt == 0)
                        _attachedAccounts.Remove(accId);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to detach account {accId}");
                }
                return false;
            }


            private class AttachedAccount
            {
                private readonly Impl _runtime;
                private readonly IAccountProxy _account;
                private readonly string _accId;
                private readonly object _lock = new object();
                private int _refCnt;


                public AttachedAccount(Impl runtime, IAccountProxy account)
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
                    lock (_lock)
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
}
