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
using TickTrader.Algo.Runtime;

namespace TickTrader.Algo.Server
{
    internal class RuntimeControlActor : Actor
    {
        private enum RuntimeState { Stopped, Starting, Connecting, Running, Stopping };

        public const int ConnectTimeout = 5000;
        public const int ShutdownTimeout = 10000;
        public const int KillTimeout = 5000;

        private static readonly TimeSpan KeepAliveThreshold = TimeSpan.FromMinutes(2);

        private readonly IRuntimeOwner _owner;
        private readonly RuntimeConfig _config;
        private readonly PackageInfo _pkgInfo;
        private readonly string _id, _pkgId;
        private readonly Dictionary<string, IActorRef> _pluginsMap = new Dictionary<string, IActorRef>();

        private IAlgoLogger _logger;
        private RuntimeState _state;
        private Process _process;
        private TaskCompletionSource<object> _connectTaskSrc, _shutdownTaskSrc;
        private CancellationTokenSource _killProcessCancelSrc;
        private IRuntimeProxy _proxy;
        private RpcSession _session;
        private IDisposable _sessionStateChangedSub;
        private int _activeExecutorsCnt;
        private bool _isObsolete, _isInvalid;
        private DateTime _pendingShutdown;
        private ActorGate _requestGate;
        private ActorLock _controlLock;
        private IDisposable _startLockToken, _shutdownLockToken;

        public RuntimeControlActor(IRuntimeOwner owner, RuntimeConfig config, PackageInfo pkgInfo)
        {
            _owner = owner;
            _config = config;
            _pkgInfo = pkgInfo;
            _id = config.Id;
            _pkgId = config.PackageId;

            Receive<ShutdownCmd>(Shutdown);
            Receive<MarkObsoleteCmd>(MarkObsolete);
            Receive<ConnectSessionCmd, bool>(ConnectSession);
            Receive<StartExecutorRequest>(StartExecutor);
            Receive<StopExecutorRequest>(StopExecutor);
            Receive<AttachPluginCmd, bool>(AttachPlugin);
            Receive<DetachPluginCmd, bool>(DetachPlugin);
            Receive<GetPluginInfoRequest, PluginInfo>(GetPluginInfo);
            Receive<ExecutorNotificationMsg>(OnExecutorNotification);
            Receive<ProcessExitedMsg>(OnProcessExited);

            Receive<ManageRuntimeCmd>(ManageRuntimeLoop);
            Receive<ScheduleShutdownCmd>(ScheduleShutdown);
            Receive<ConnectionLostMsg>(OnConnectionLost);
        }


        public static IActorRef Create(IRuntimeOwner owner, RuntimeConfig config, PackageInfo pkgInfo)
        {
            return ActorSystem.SpawnLocal(() => new RuntimeControlActor(owner, config, pkgInfo), $"{nameof(RuntimeControlActor)} ({config.Id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
            _state = RuntimeState.Stopped;

            _controlLock = CreateLock();
            _requestGate = CreateGate();
            _requestGate.OnWait += () => Self.Tell(ManageRuntimeCmd.Instance);
            _requestGate.OnExit += () => Self.Tell(ScheduleShutdownCmd.Instance);

            Self.Tell(ManageRuntimeCmd.Instance);
        }


        private Task Shutdown(ShutdownCmd cmd)
        {
            if (_shutdownTaskSrc == null)
            {
                _shutdownTaskSrc = new TaskCompletionSource<object>();
                ShutdownInternal("Server shutdown").Forget();
            }

            return _shutdownTaskSrc.Task;
        }

        private void MarkObsolete(MarkObsoleteCmd cmd)
        {
            _isObsolete = true;
            _logger.Debug("Marked obsolete");

            NotifyServerOfCompleteShutdown();

            ManageRuntimeInternal();
        }

        private async Task StartExecutor(StartExecutorRequest request)
        {
            using (await _requestGate.Enter())
            {
                ThrowIfNotRunning();

                _activeExecutorsCnt++;
                await _proxy.StartExecutor(request);
                _logger.Debug($"Executor {request.Config.Id} started. Have {_activeExecutorsCnt} active executors");
            }
        }

        private async Task StopExecutor(StopExecutorRequest request)
        {
            using (await _requestGate.Enter())
            {
                ThrowIfNotRunning();

                await _proxy.StopExecutor(request);
            }
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
            if (_isObsolete)
            {
                _logger.Info($"Can't attach plugin '{id}'. Marked as obsolete");
                return false;
            }

            _pluginsMap.Add(id, cmd.Plugin);
            _logger.Debug($"Attached plugin '{id}'. Have {_pluginsMap.Count} attached plugins");
            ManageRuntimeInternal();
            return true;
        }

        public bool DetachPlugin(DetachPluginCmd cmd)
        {
            var id = cmd.PluginId;

            if (_pluginsMap.Remove(id))
            {
                _logger.Debug($"Detached plugin '{id}'. Have {_pluginsMap.Count} attached plugins");
                ManageRuntimeInternal();
                return true;
            }
            else
            {
                _logger.Error($"Plugin '{id}' was not attached");
                return false;
            }
        }


        private void ManageRuntimeLoop(ManageRuntimeCmd cmd)
        {
            if (_shutdownTaskSrc != null)
                return;

            ManageRuntimeInternal();
            TaskExt.Schedule(1000, () => Self.Tell(ManageRuntimeCmd.Instance), StopToken);
        }

        private void ScheduleShutdown(ScheduleShutdownCmd cmd)
        {
            _pendingShutdown = DateTime.UtcNow + KeepAliveThreshold;
        }

        private void ManageRuntimeInternal()
        {
            if (_state == RuntimeState.Stopped)
            {
                if (!_requestGate.IsClosed)
                    return;

                var devModeStart = _pluginsMap.Count > 0 && _owner.EnableDevMode;
                var forcedStart = _requestGate.WatingCount > 0;

                if (devModeStart || forcedStart)
                {
                    _logger.Debug($"Staring runtime... ({nameof(devModeStart)} = {devModeStart}, {nameof(forcedStart)} = {forcedStart})");
                    StartInternal().OnException(ex => _logger.Error(ex, "Start failed"));
                }
            }
            else if (_state == RuntimeState.Running)
            {
                var shouldBeRunning = _activeExecutorsCnt > 0 || (_pluginsMap.Count > 0 && _owner.EnableDevMode);
                var scheduledShutdown = !shouldBeRunning && _pendingShutdown < DateTime.UtcNow;
                var obsoleteShutdown = !shouldBeRunning && _isObsolete;

                var reason = string.Empty;
                if (scheduledShutdown)
                    reason = "Idle shutdown";
                else if (obsoleteShutdown)
                    reason = "Obsolete shutdown";

                if (scheduledShutdown || obsoleteShutdown)
                    ShutdownInternal(reason).OnException(ex => _logger.Error(ex, "Shutdown failed"));
            }
        }

        private async Task StartInternal()
        {
            _startLockToken = await _controlLock.GetLock(nameof(StartInternal));

            try
            {
                ChangeState(RuntimeState.Starting);

                var started = StartProcess();
                if (!started)
                    return;

                ChangeState(RuntimeState.Connecting);

                _connectTaskSrc = new TaskCompletionSource<object>();
                var connected = await _connectTaskSrc.Task.WaitAsync(ConnectTimeout);

                if (!connected)
                {
                    _logger.Error("Failed to connect runtime host within timeout");
                    ScheduleKillProcess();
                    return;
                }

                await _proxy.Start(new StartRuntimeRequest { Config = _config });

                ScheduleShutdown(ScheduleShutdownCmd.Instance);

                _startLockToken.Dispose();
                _startLockToken = null;

                ChangeState(RuntimeState.Running);

                _requestGate.Open();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start");

                if (_process != null && !_process.HasExited)
                    ScheduleKillProcess();
            }
        }

        private bool StartProcess()
        {
            var rpcParams = _owner.GetRpcParams();
            rpcParams.ProxyId = _id;
            var startInfo = new ProcessStartInfo(_owner.RuntimeExePath)
            {
                UseShellExecute = false,
                WorkingDirectory = _owner.WorkingDirectory,
                CreateNoWindow = true,
            };
            rpcParams.SaveAsEnvVars(startInfo.Environment);

            _process = Process.Start(startInfo);
            _process.Exited += OnProcessExit;
            _process.EnableRaisingEvents = true;

            _logger.Info($"Runtime host started in process {_process.Id}");

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
                _logger.Info($"Runtime host didn't stop within timeout. Killing process {_process.Id}...");
                _process.Kill();
            }, _killProcessCancelSrc.Token);
        }

        private void OnProcessExited(ProcessExitedMsg msg)
        {
            _process.Exited -= OnProcessExit;
            _killProcessCancelSrc?.Cancel();

            var crashed = msg.ExitCode != 0 || _state != RuntimeState.Stopping;

            var oldState = _state;
            ChangeState(RuntimeState.Stopped);
            ManageRequestGate(oldState).Forget();
            DeinitSession();

            _shutdownTaskSrc?.TrySetResult(null);
            NotifyServerOfCompleteShutdown();

            if (_startLockToken != null)
            {
                _startLockToken.Dispose();
                _startLockToken = null;
            }
            if (_shutdownLockToken != null)
            {
                _shutdownLockToken.Dispose();
                _shutdownLockToken = null;
            }

            if (crashed)
            {
                _activeExecutorsCnt = 0;
                NotifyAttachedPlugins(RuntimeControlModel.RuntimeCrashedMsg.Instance);
            }
        }

        private void OnProcessExit(object sender, EventArgs args)
        {
            // runs directly on thread pool
            try
            {
                // non-actor context
                var exitCode = _process.ExitCode;
                _logger.Info($"Process {_process.Id} exited with exit code {exitCode}");
                Self.Tell(new ProcessExitedMsg(exitCode));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Process exit handler failed");
            }
        }

        private async Task ManageRequestGate(RuntimeState oldState)
        {
            switch (oldState)
            {
                case RuntimeState.Starting:
                case RuntimeState.Connecting:
                    _logger.Debug("Run queued gate requests...");
                    await _requestGate.ExecQueuedRequests();
                    _logger.Debug("Closed gate");
                    break;
                case RuntimeState.Running:
                    _logger.Debug("Closing gate...");
                    await _requestGate.Close();
                    _logger.Debug("Closed gate");
                    break;
            }
        }

        private async Task ShutdownInternal(string reason)
        {
            _shutdownLockToken = await _controlLock.GetLock(nameof(ShutdownInternal));
            if (_state == RuntimeState.Stopped)
            {
                _shutdownTaskSrc?.TrySetResult(null);
                return;
            }

            try
            {
                _logger.Debug($"Shutdown reason: {reason}");
                ChangeState(RuntimeState.Stopping);

                _logger.Debug("Closing gate...");
                await _requestGate.Close();

                _logger.Debug("Stoppping runtime host...");
                var finished = await _proxy.Stop(new StopRuntimeRequest()).WaitAsync(ShutdownTimeout);
                if (!finished)
                    _logger.Error("No response for stop request. Considering runtime host hanged");

                _logger.Debug("Disconnecting...");
                await _session.Disconnect(reason);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to shutdown");
            }
            finally
            {
                ScheduleKillProcess();
            }
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
            _sessionStateChangedSub = session.StateChanged.Subscribe(_ => Self.Tell(ConnectionLostMsg.Instance));

            _connectTaskSrc?.TrySetResult(null);

            return true;
        }

        private void OnConnectionLost(ConnectionLostMsg msg)
        {
            DeinitSession();

            if (_state != RuntimeState.Stopping && _state != RuntimeState.Stopped)
            {
                _logger.Info("Connection to runtime host lost");
                ScheduleKillProcess();
            }
        }

        private void DeinitSession()
        {
            if (_session == null)
                return;

            _logger.Debug("Deinit session");

            _proxy = null;
            _session = null;
            _sessionStateChangedSub?.Dispose();
            _sessionStateChangedSub = null;
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
            else if (payload.Is(OutputSeriesUpdate.Descriptor))
                plugin.Tell(payload.Unpack<OutputSeriesUpdate>());
            else if (payload.Is(PluginExitedMsg.Descriptor))
                plugin.Tell(payload.Unpack<PluginExitedMsg>());
            else if (payload.Is(PluginAbortedMsg.Descriptor))
                OnPluginAborted(payload.Unpack<PluginAbortedMsg>());
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

            ManageRuntimeInternal();
        }


        private void ThrowIfNotRunning()
        {
            if (_state != RuntimeState.Running)
                throw Errors.RuntimeNotStarted(_id);
        }

        private void ChangeState(RuntimeState newState)
        {
            _logger.Debug($"State changed: {_state} -> {newState}");
            _state = newState;
        }

        private void NotifyServerOfCompleteShutdown()
        {
            if (_isObsolete && _state == RuntimeState.Stopped && _pluginsMap.Count == 0)
            {
                _owner.OnRuntimeStopped(_id);
            }
        }

        private void NotifyAttachedPlugins(object msg)
        {
            foreach (var plugin in _pluginsMap.Values)
                plugin.Tell(msg);
        }

        private void OnPluginAborted(PluginAbortedMsg msg)
        {
            if (!_isInvalid)
            {
                _logger.Debug($"Plugin '{msg.Id}' aborted. Entering invalid state");

                _isInvalid = true;
                _owner.OnRuntimeInvalid(_pkgId, _id);
                NotifyAttachedPlugins(RuntimeControlModel.RuntimeInvalidMsg.Instance);
            }
        }


        internal sealed class ShutdownCmd : Singleton<ShutdownCmd> { }

        internal sealed class MarkObsoleteCmd : Singleton<MarkObsoleteCmd> { }

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


        private sealed class ManageRuntimeCmd : Singleton<ManageRuntimeCmd> { }

        private sealed class ScheduleShutdownCmd : Singleton<ScheduleShutdownCmd> { }

        private sealed class ConnectionLostMsg : Singleton<ConnectionLostMsg> { }
    }
}
