using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    internal class PkgRuntimeActor : Actor
    {
        public const int AttachTimeout = 5000;
        public const int ShutdownTimeout = 10000;

        private readonly IAlgoLogger _logger;
        private readonly string _id, _pkgId, _pkgRefId;
        private readonly AlgoServer _server;
        private readonly IRuntimeHostProxy _runtimeHost;
        private readonly Dictionary<string, ExecutorModel> _executorsMap = new Dictionary<string, ExecutorModel>();

        private TaskCompletionSource<bool> _startTaskSrc, _connectTaskSrc;
        private Action<RpcMessage> _onNotification;
        private IRuntimeProxy _proxy;
        private RpcSession _session;
        private int _startedExecutorsCnt;
        private bool _shutdownWhenIdle;

        public PkgRuntimeActor(string id, string pkgId, string pkgRefId, AlgoServer server)
        {
            _id = id;
            _pkgId = pkgId;
            _pkgRefId = pkgRefId;
            _server = server;

            _logger = AlgoLoggerFactory.GetLogger($"{nameof(PkgRuntimeModel)}({_id})");
            _runtimeHost = RuntimeHost.Create(true);

            Receive<StartRuntimeCmd, bool>(Start);
            Receive<StopRuntimeCmd>(Stop);
            Receive<MarkForShutdownCmd>(MarkForShutdown);
            Receive<ConnectSessionCmd, bool>(OnConnect);
            Receive<StartExecutorRequest>(StartExecutor);
            Receive<StopExecutorRequest>(StopExecutor);
            Receive<RuntimeConfigRequest, RuntimeConfig>(GetConfig);
            Receive<GetPluginInfoRequest, PluginInfo>(GetPluginInfo);
            Receive<CreateExecutorCmd, ExecutorModel>(CreateExecutor);
            Receive<DisposeExecutorCmd>(DisposeExecutor);
            Receive<ExecutorConfigRequest, ExecutorConfig>(GetExecutorConfig);
            Receive<ExecutorNotificationMsg>(OnExecutorNotification);
        }


        public static IActorRef Create(string id, string pkgId, string pkgRefId, AlgoServer server)
        {
            return ActorSystem.SpawnLocal(() => new PkgRuntimeActor(id, pkgId, pkgRefId, server), $"{nameof(PkgRuntimeActor)} {id}");
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
                    await _proxy.Start(new StartRuntimeRequest());

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

                var finished = await _proxy.Stop(new StopRuntimeRequest()).WaitAsync(ShutdownTimeout);
                if (!finished)
                    _logger.Error("No response for stop request. Considering process hanged");

                await _session.Disconnect(reason);
                OnDetached();
                await _runtimeHost.Stop();

                _server.Runtimes.OnRuntimeStopped(_id);

                _logger.Debug("Stopped");
            }
            catch (Exception ex)
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
            await WhenStarted();

            await _proxy.StartExecutor(request);
            _startedExecutorsCnt++;
            _logger.Debug($"Executor {request.ExecutorId} started. Have {_startedExecutorsCnt} active executors");
        }

        private async Task StopExecutor(StopExecutorRequest request)
        {
            await WhenStarted();

            await _proxy.StopExecutor(request);
        }

        private PluginInfo GetPluginInfo(GetPluginInfoRequest request)
        {
            return new PluginInfo(request.Plugin, null);
        }

        private Task<ExecutorModel> CreateExecutor(CreateExecutorCmd cmd)
        {
            var id = cmd.ExecutorId;
            if (_executorsMap.ContainsKey(id))
                return Task.FromException<ExecutorModel>(Errors.DuplicateExecutorId(id));

            var executor = new ExecutorModel(new PkgRuntimeModel(Self), id, cmd.Config);
            _executorsMap.Add(id, executor);
            return Task.FromResult(executor);
        }

        private void DisposeExecutor(DisposeExecutorCmd cmd)
        {
            _executorsMap.Remove(cmd.ExecutorId);
        }

        private ExecutorConfig GetExecutorConfig(ExecutorConfigRequest request)
        {
            var id = request.ExecutorId;
            if (!_executorsMap.TryGetValue(id, out var executor))
                throw Errors.ExecutorNotFound(id);

            return executor.Config;
        }

        private void OnExecutorNotification(ExecutorNotificationMsg msg)
        {
            var id = msg.ExecutorId;
            var payload = msg.Payload;

            if (payload.Is(ExecutorErrorMsg.Descriptor))
                PluginErrorHandler(id, payload);

            if (!_executorsMap.TryGetValue(id, out var executor))
                _logger.Error($"Executor {id} not found. Notification type {msg.Payload.TypeUrl}");

            if (payload.Is(ExecutorStateUpdate.Descriptor))
                ExecutorStateUpdateHandler(executor, payload.Unpack<ExecutorStateUpdate>());
            else if (payload.Is(PluginLogRecord.Descriptor))
                executor.OnLogUpdated(payload.Unpack<PluginLogRecord>());
            else if (payload.Is(PluginStatusUpdate.Descriptor))
                executor.OnStatusUpdated(payload.Unpack<PluginStatusUpdate>());
            else if (payload.Is(DataSeriesUpdate.Descriptor))
                executor.OnDataSeriesUpdate(payload.Unpack<DataSeriesUpdate>());
            else if (payload.Is(PluginExitedMsg.Descriptor))
                executor.OnExit(payload.Unpack<PluginExitedMsg>());
        }

        private void PluginErrorHandler(string id, Any payload)
        {
            var error = payload.Unpack<ExecutorErrorMsg>();
            _logger.Error(new AlgoPluginException(error), $"Exception in executor {id}");
        }

        private void ExecutorStateUpdateHandler(ExecutorModel executor, ExecutorStateUpdate update)
        {
            if (update.NewState.IsFaulted() || update.NewState.IsStopped())
                OnExecutorStopped(update.ExecutorId);

            executor.OnStateUpdated(update);
        }


        private void OnExecutorStopped(string executorId)
        {
            _startedExecutorsCnt--;
            _logger.Debug($"Executor {executorId} stopped. Have {_startedExecutorsCnt} active executors");

            _executorsMap.Remove(executorId);

            if (_startedExecutorsCnt == 0 && _shutdownWhenIdle)
                ShutdownInternal();
        }


        private async Task WhenStarted()
        {
            if (_startTaskSrc == null)
                throw Errors.RuntimeNotStarted(_id);

            var connected = await _startTaskSrc.Task;
            if (!connected)
                throw Errors.RuntimeNotStarted(_id);
        }


        internal class StartRuntimeCmd { }

        internal class StopRuntimeCmd
        {
            public string Reason { get; }

            public StopRuntimeCmd(string reason)
            {
                Reason = reason;
            }
        }

        internal class MarkForShutdownCmd { }

        internal class ConnectSessionCmd
        {
            public RpcSession Session { get; }

            public ConnectSessionCmd(RpcSession session)
            {
                Session = session;
            }
        }

        internal class GetPluginInfoRequest
        {
            public PluginKey Plugin { get; }

            public GetPluginInfoRequest(PluginKey plugin)
            {
                Plugin = plugin;
            }
        }

        internal class CreateExecutorCmd
        {
            public string ExecutorId { get; }

            public ExecutorConfig Config { get; }

            public CreateExecutorCmd(string executorId, ExecutorConfig config)
            {
                ExecutorId = executorId;
                Config = config;
            }
        }

        internal class DisposeExecutorCmd
        {
            public string ExecutorId { get; }

            public DisposeExecutorCmd(string executorId)
            {
                ExecutorId = executorId;
            }
        }

        internal class ExecutorConfigRequest
        {
            public string ExecutorId { get; }

            public ExecutorConfigRequest(string executorId)
            {
                ExecutorId = executorId;
            }
        }

        internal class ExecutorNotificationMsg
        {
            public string ExecutorId { get; }

            public Any Payload { get; }

            public ExecutorNotificationMsg(string executorId, Any payload)
            {
                ExecutorId = executorId;
                Payload = payload;
            }
        }
    }
}
