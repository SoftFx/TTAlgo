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
    internal class RuntimeControlActor : Actor
    {
        public const int AttachTimeout = 5000;
        public const int ShutdownTimeout = 10000;

        private readonly AlgoServerPrivate _server;
        private readonly RuntimeConfig _config;
        private readonly string _id, _pkgId;
        private readonly IRuntimeHostProxy _runtimeHost;
        private readonly Dictionary<string, IActorRef> _pluginsMap = new Dictionary<string, IActorRef>();

        private IAlgoLogger _logger;
        private TaskCompletionSource<bool> _startTaskSrc, _connectTaskSrc;
        private Action<RpcMessage> _onNotification;
        private IRuntimeProxy _proxy;
        private RpcSession _session;
        private int _activeExecutorsCnt;
        private bool _shutdownWhenIdle;

        public RuntimeControlActor(AlgoServerPrivate server, RuntimeConfig config)
        {
            _server = server;
            _config = config;
            _id = config.Id;
            _pkgId = config.PackageId;

            _runtimeHost = RuntimeHost.Create(true);

            Receive<StartRuntimeCmd, bool>(Start);
            Receive<StopRuntimeCmd>(Stop);
            Receive<MarkForShutdownCmd>(MarkForShutdown);
            Receive<ConnectSessionCmd, bool>(ConnectSession);
            Receive<StartExecutorRequest>(StartExecutor);
            Receive<StopExecutorRequest>(StopExecutor);
            Receive<AttachPluginCmd, bool>(AttachPlugin);
            Receive<DetachPluginCmd, bool>(DetachPlugin);
            Receive<GetPluginInfoRequest, PluginInfo>(GetPluginInfo);
            Receive<ExecutorNotificationMsg>(OnExecutorNotification);
        }


        public static IActorRef Create(AlgoServerPrivate server, RuntimeConfig config)
        {
            return ActorSystem.SpawnLocal(() => new RuntimeControlActor(server, config), $"{nameof(RuntimeControlActor)} ({config.Id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
        }


        private async Task<bool> Start(StartRuntimeCmd cmd)
        {
            if (_startTaskSrc != null)
                return await _startTaskSrc.Task;

            _startTaskSrc = new TaskCompletionSource<bool>();

            _logger.Debug("Starting...");

            try
            {
                _connectTaskSrc = new TaskCompletionSource<bool>();
                await _runtimeHost.Start(_server.Address, _server.BoundPort, _id);

                TaskExt.Schedule(AttachTimeout, () => _connectTaskSrc?.TrySetResult(false));
                var connected = await _connectTaskSrc.Task;
                _connectTaskSrc = null;

                if (connected)
                {
                    await _proxy.Start(new StartRuntimeRequest { Config = _config });

                    _startTaskSrc.TrySetResult(true);
                    _logger.Debug("Started");
                    return true;
                }
                else
                {
                    _logger.Error("Failed to connect runtime host");
                    await _runtimeHost.Stop();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start");
            }

            _startTaskSrc.TrySetResult(false);
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

                _server.OnRuntimeStopped(_id);

                _logger.Debug("Stopped");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop");
            }
        }

        private void MarkForShutdown(MarkForShutdownCmd cmd)
        {
            _shutdownWhenIdle = true;
            _logger.Debug("Marked for shutdown");
            if (_activeExecutorsCnt == 0)
                ShutdownInternal();
        }

        private bool ConnectSession(ConnectSessionCmd cmd)
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

            _activeExecutorsCnt++;
            await _proxy.StartExecutor(request);
            _logger.Debug($"Executor {request.Config.Id} started. Have {_activeExecutorsCnt} active executors");
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

        private bool AttachPlugin(AttachPluginCmd cmd)
        {
            var id = cmd.PluginId;
            if (_pluginsMap.ContainsKey(id))
            {
                _logger.Error($"Plugin '{id}' already attached");
                return false;
            }

            _pluginsMap.Add(id, cmd.Plugin);
            _logger.Debug($"Attached plugin '{id}'. Have {_pluginsMap.Count} attached plugins");
            return true;
        }

        public bool DetachPlugin(DetachPluginCmd cmd)
        {
            var id = cmd.PluginId;

            if (_pluginsMap.Remove(id))
            {
                _logger.Debug($"Detached plugin '{id}'. Have {_pluginsMap.Count} attached plugins");
                return true;
            }
            else
            {
                _logger.Error($"Plugin '{id}' was not attached");
                return false;
            }
        }

        private void OnExecutorNotification(ExecutorNotificationMsg msg)
        {
            var id = msg.ExecutorId;
            var payload = msg.Payload;

            if (payload.Is(ExecutorErrorMsg.Descriptor))
                PluginErrorHandler(id, payload);

            if (!_pluginsMap.TryGetValue(id, out var plugin))
            {
                _logger.Error($"Executor {id} not found. Notification type {msg.Payload.TypeUrl}");
                return;
            }

            if (payload.Is(ExecutorStateUpdate.Descriptor))
                ExecutorStateUpdateHandler(plugin, payload.Unpack<ExecutorStateUpdate>());
            else if (payload.Is(PluginLogRecord.Descriptor))
                plugin.Tell(payload.Unpack<PluginLogRecord>());
            else if (payload.Is(PluginStatusUpdate.Descriptor))
                plugin.Tell(payload.Unpack<PluginStatusUpdate>());
            else if (payload.Is(DataSeriesUpdate.Descriptor))
                plugin.Tell(payload.Unpack<DataSeriesUpdate>());
            else if (payload.Is(PluginExitedMsg.Descriptor))
                plugin.Tell(payload.Unpack<PluginExitedMsg>());
        }

        private void PluginErrorHandler(string id, Any payload)
        {
            var error = payload.Unpack<ExecutorErrorMsg>();
            _logger.Error(new AlgoPluginException(error), $"Exception in executor {id}");
        }

        private void ExecutorStateUpdateHandler(IActorRef plugin, ExecutorStateUpdate update)
        {
            if (update.NewState.IsFaulted() || update.NewState.IsStopped())
                OnExecutorStopped(update.ExecutorId);

            plugin.Tell(update);
        }


        private void OnExecutorStopped(string executorId)
        {
            _activeExecutorsCnt--;
            _logger.Debug($"Executor {executorId} stopped. Have {_activeExecutorsCnt} active executors");

            if (_activeExecutorsCnt == 0 && _shutdownWhenIdle)
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

        internal class AttachPluginCmd
        {
            public string PluginId { get; }

            public IActorRef Plugin { get; }

            public AttachPluginCmd(string pluginId, IActorRef plugin)
            {
                PluginId = pluginId;
                Plugin = plugin;
            }
        }

        internal class DetachPluginCmd
        {
            public string PluginId { get; }

            public DetachPluginCmd(string pluginId)
            {
                PluginId = pluginId;
            }
        }
    }
}
