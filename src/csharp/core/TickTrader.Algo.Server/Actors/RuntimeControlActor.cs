using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
        private enum RuntimeState { Stopped, Starting, Connecting, Running, Stopping };

        public const int ConnectTimeout = 5000;
        public const int ShutdownTimeout = 10000;
        public const int KillTimeout = 2000;

        private readonly AlgoServerPrivate _server;
        private readonly RuntimeConfig _config;
        private readonly PackageInfo _pkgInfo;
        private readonly string _id, _pkgId;
        private readonly Dictionary<string, IActorRef> _pluginsMap = new Dictionary<string, IActorRef>();

        private IAlgoLogger _logger;
        private RuntimeState _state;
        private Process _process;
        private TaskCompletionSource<bool> _startTaskSrc;
        private TaskCompletionSource<object> _connectTaskSrc, _stopTaskSrc;
        private CancellationTokenSource _killProcessCancelSrc;
        private IRuntimeProxy _proxy;
        private RpcSession _session;
        private int _activeExecutorsCnt;
        private bool _shutdownWhenIdle;

        public RuntimeControlActor(AlgoServerPrivate server, RuntimeConfig config, PackageInfo pkgInfo)
        {
            _server = server;
            _config = config;
            _pkgInfo = pkgInfo;
            _id = config.Id;
            _pkgId = config.PackageId;

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
            Receive<ProcessExitedMsg>(OnProcessExited);
        }


        public static IActorRef Create(AlgoServerPrivate server, RuntimeConfig config, PackageInfo pkgInfo)
        {
            return ActorSystem.SpawnLocal(() => new RuntimeControlActor(server, config, pkgInfo), $"{nameof(RuntimeControlActor)} ({config.Id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
            _state = RuntimeState.Stopped;
        }


        private async Task<bool> Start(StartRuntimeCmd cmd)
        {
            if (_state == RuntimeState.Running)
                return true;
            if (_state != RuntimeState.Stopped)
                return await _startTaskSrc.Task;

            _startTaskSrc = new TaskCompletionSource<bool>();
            var res = await StartInternal();
            _startTaskSrc.TrySetResult(res);
            return res;
        }

        private async Task<bool> StartInternal()
        {
            try
            {
                ChangeState(RuntimeState.Starting);

                var started = StartProcess();
                if (!started)
                    return false;

                ChangeState(RuntimeState.Connecting);

                _connectTaskSrc = new TaskCompletionSource<object>();
                var connected = await _connectTaskSrc.Task.WaitAsync(ConnectTimeout);

                if (!connected)
                {
                    _logger.Error("Failed to connect runtime process within timeout");
                    ScheduleKillProcess();
                    return false;
                }

                await _proxy.Start(new StartRuntimeRequest { Config = _config });

                ChangeState(RuntimeState.Running);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start");

                if (_process != null && !_process.HasExited)
                    ScheduleKillProcess();
            }

            return false;
        }

        private bool StartProcess()
        {
            var startInfo = new ProcessStartInfo(_server.Env.RuntimeExePath)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = string.Join(" ", _server.Address, _server.BoundPort.ToString(), $"\"{_id}\""),
                WorkingDirectory = _server.Env.AppFolder,
            };

            _process = Process.Start(startInfo);
            _process.Exited += OnProcessExit;
            _process.EnableRaisingEvents = true;

            _logger.Info($"Runtime started in process {_process.Id}");

            if (_process.HasExited) // If event was enabled after actual stop
            {
                OnProcessExit(_process, null);
                return false;
            }

            return true;
        }

        private void ScheduleKillProcess()
        {
            _killProcessCancelSrc = new CancellationTokenSource();
            TaskExt.Schedule(KillTimeout, () =>
            {
                _logger.Info($"Runtime process didn't stop within timeout. Killing process {_process.Id}...");
                _process.Kill();
            }, _killProcessCancelSrc.Token);
        }

        private void OnProcessExited(ProcessExitedMsg msg)
        {
            _process.Exited -= OnProcessExit;
            _killProcessCancelSrc?.Cancel();

            var crashed = msg.ExitCode != 0 || _state != RuntimeState.Stopping;

            ChangeState(RuntimeState.Stopped);
            _server.OnRuntimeStopped(_id);

            if (crashed)
            {
                foreach (var plugin in _pluginsMap.Values)
                    plugin.Tell(PluginActor.RuntimeCrashedMsg.Instance);
            }
        }

        private void OnProcessExit(object sender, EventArgs args)
        {
            // non-actor context
            var exitCode = _process.ExitCode;
            _logger.Info($"Process {_process.Id} exited with exit code {exitCode}");
            Self.Tell(new ProcessExitedMsg(exitCode));
        }

        private async Task Stop(StopRuntimeCmd cmd)
        {
            if (_state == RuntimeState.Stopped)
                return;
            if (_state == RuntimeState.Stopping)
            {
                await _stopTaskSrc.Task;
                return;
            }
            if (_state != RuntimeState.Running)
            {
                await _startTaskSrc.Task;
                await Stop(cmd);
                return;
            }

            _stopTaskSrc = new TaskCompletionSource<object>();

            await StopInternal(cmd.Reason);

            _stopTaskSrc.TrySetResult(null);
        }

        private async Task StopInternal(string reason)
        {
            try
            {
                _logger.Debug($"Stop reason: {reason}");
                ChangeState(RuntimeState.Stopping);

                await _startTaskSrc.Task;
                _startTaskSrc = null;

                var finished = await _proxy.Stop(new StopRuntimeRequest()).WaitAsync(ShutdownTimeout);
                if (!finished)
                    _logger.Error("No response for stop request. Considering process hanged");

                await _session.Disconnect(reason);
                OnDetached();

                ScheduleKillProcess();
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

            _proxy = new RemoteRuntimeProxy(session);
            _session = session;

            _connectTaskSrc?.TrySetResult(null);

            return true;
        }

        private void OnDetached()
        {
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
            return _pkgInfo.GetPlugin(request.Plugin);
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

        private void ChangeState(RuntimeState newState)
        {
            _logger.Debug($"State changed: {_state} -> {newState}");
            _state = newState;
        }


        internal class StartRuntimeCmd : Singleton<StartRuntimeCmd> { }

        internal class StopRuntimeCmd
        {
            public string Reason { get; }

            public StopRuntimeCmd(string reason)
            {
                Reason = reason;
            }
        }

        internal class MarkForShutdownCmd : Singleton<MarkForShutdownCmd> { }

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

        internal class ProcessExitedMsg
        {
            public int ExitCode { get; }

            public ProcessExitedMsg(int exitCode)
            {
                ExitCode = exitCode;
            }
        }
    }
}
