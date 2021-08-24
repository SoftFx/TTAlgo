using Google.Protobuf.WellKnownTypes;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class PluginActor : Actor
    {
        private readonly AlgoServerPrivate _server;
        private readonly string _id, _accId;
        private readonly ActorEventSource<PluginLogRecord> _logEventSrc = new ActorEventSource<PluginLogRecord>();
        private readonly ActorEventSource<PluginStatusUpdate> _statusEventSrc = new ActorEventSource<PluginStatusUpdate>();
        private readonly ActorEventSource<DataSeriesUpdate> _outputEventSrc = new ActorEventSource<DataSeriesUpdate>();

        private PluginSavedState _savedState;
        private IAlgoLogger _logger;
        private PluginConfig _config;

        private PluginModelInfo.Types.PluginState _state;
        private IActorRef _runtime;
        private PluginInfo _pluginInfo;
        private string _faultMsg, _newestRuntimeId, _currentRuntimeId;
        private TaskCompletionSource<bool> _startTaskSrc, _stopTaskSrc;
        private MessageCache<PluginLogRecord> _logsCache;
        private PluginStatusUpdate _lastStatus;
        private ActorLock _runtimeLock;


        private PluginActor(AlgoServerPrivate server, PluginSavedState savedState)
        {
            _server = server;
            _savedState = savedState;
            _id = savedState.Id;
            _accId = savedState.AccountId;

            Receive<StartCmd>(Start);
            Receive<StopCmd>(Stop);
            Receive<UpdateConfigCmd>(UpdateConfig);
            Receive<AttachLogsChannelCmd>(AttachLogsChannel);
            Receive<AttachStatusChannelCmd>(AttachStatusChannel);
            Receive<AttachOutputsChannelCmd>(cmd => _outputEventSrc.Subscribe(cmd.OutputSink));
            Receive<PluginLogsRequest, PluginLogRecord[]>(GetLogs);
            Receive<PluginStatusRequest, string>(GetStatus);

            Receive<PluginLogRecord>(OnLogUpdated);
            Receive<PluginStatusUpdate>(OnStatusUpdated);
            Receive<DataSeriesUpdate>(update => _outputEventSrc.DispatchEvent(update));
            Receive<ExecutorStateUpdate>(OnExecutorStateUpdated);
            Receive<PluginExitedMsg>(OnExited);
            Receive<RuntimeCrashedMsg>(OnRuntimeCrashed);
            Receive<AlgoServerActor.PkgRuntimeUpdate>(OnPkgRuntimeUpdated);
        }


        public static IActorRef Create(AlgoServerPrivate server, PluginSavedState savedState)
        {
            return ActorSystem.SpawnLocal(() => new PluginActor(server, savedState), $"{nameof(PluginActor)} ({savedState.Id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
            _logsCache = new MessageCache<PluginLogRecord>(100);
            _lastStatus = new PluginStatusUpdate { PluginId = _id, Message = string.Empty };

            _config = _savedState.UnpackConfig();

            _server.SendUpdate(PluginModelUpdate.Added(_id, GetInfoCopy()));
            _runtimeLock = CreateLock();

            _server.SendPkgRuntimeUpdate(_config.Key.PackageId, Self);
        }


        private async Task Start(StartCmd cmd)
        {
            using (await _runtimeLock.GetLock(nameof(Start)))
            {
                if (_state.IsRunning())
                    return;

                await StartInternal();
            }
        }

        private async Task Stop(StopCmd cmd)
        {
            using (await _runtimeLock.GetLock(nameof(Stop)))
            {
                if (_state.IsStopped())
                    return;

                await StopInternal();
            }
        }

        private async Task UpdateConfig(UpdateConfigCmd cmd)
        {
            if (_state.IsRunning())
                throw Errors.PluginIsRunning(_id);

            _savedState.PackConfig(cmd.NewConfig);
            await _server.SavedState.UpdatePlugin(_savedState);
            _config = cmd.NewConfig;

            _server.SendUpdate(PluginModelUpdate.Updated(_id, GetInfoCopy()));
        }

        private void AttachLogsChannel(AttachLogsChannelCmd cmd)
        {
            var sink = cmd.LogSink;
            if (sink == null)
                return;

            if (cmd.SendSnapshot)
                _logsCache.SendSnapshot(sink);

            _logEventSrc.Subscribe(sink);
        }

        private void AttachStatusChannel(AttachStatusChannelCmd cmd)
        {
            var sink = cmd.StatusSink;
            if (sink == null)
                return;

            if (cmd.SendSnapshot)
                sink.TryWrite(_lastStatus);

            _statusEventSrc.Subscribe(sink);
        }

        private PluginLogRecord[] GetLogs(PluginLogsRequest request)
        {
            return _logsCache.Where(u => u.TimeUtc > request.LastLogTimeUtc).Take(request.MaxCount).ToArray();
        }

        private string GetStatus(PluginStatusRequest request)
        {
            return _lastStatus.Message;
        }

        private void OnLogUpdated(PluginLogRecord log)
        {
            _logsCache.Add(log);
            _logEventSrc.DispatchEvent(log);
            if (log.Severity == PluginLogRecord.Types.LogSeverity.Alert)
                _server.Alerts.SendPluginAlert(_id, log);
        }

        private void OnStatusUpdated(PluginStatusUpdate update)
        {
            _lastStatus = update;
            _statusEventSrc.DispatchEvent(update);
        }

        private void OnExecutorStateUpdated(ExecutorStateUpdate update)
        {
            var newState = update.NewState;

            _logger.Debug($"Executor state: {update.OldState} -> {newState}");

            if (newState.IsWaitConnect())
                ChangeState(PluginModelInfo.Types.PluginState.Starting);
            else if (newState.IsFaulted())
            {
                ChangeState(PluginModelInfo.Types.PluginState.Faulted);
                _server.SavedState.SetPluginRunning(_id, false).Forget();
            }
            else if (newState.IsRunning())
                ChangeState(PluginModelInfo.Types.PluginState.Running);
            else if (newState.IsStopping())
                ChangeState(PluginModelInfo.Types.PluginState.Stopping);
            else if (newState.IsStopped())
                ChangeState(PluginModelInfo.Types.PluginState.Stopped);
            else if (newState.IsWaitReconnect())
                ChangeState(PluginModelInfo.Types.PluginState.Reconnecting);
        }

        private void OnExited(PluginExitedMsg msg)
        {
            _logger.Debug("Received exit notification");

            _server.SavedState.SetPluginRunning(_id, false).Forget();
        }

        private void OnRuntimeCrashed(RuntimeCrashedMsg msg)
        {
            if (!_state.IsStopped())
            {
                _logger.Info("Runtime crashed during execution");

                ChangeState(PluginModelInfo.Types.PluginState.Faulted);

                _startTaskSrc?.TrySetResult(false);
                _stopTaskSrc?.TrySetResult(false);

                _server.Alerts.SendServerAlert($"Process running plugin '{_id}' crashed");
                _server.SavedState.SetPluginRunning(_id, false).Forget();
            }
        }

        private void OnPkgRuntimeUpdated(AlgoServerActor.PkgRuntimeUpdate update)
        {
            if (update.PkgId != _config.Key.PackageId)
                return;

            _newestRuntimeId = update.RuntimeId;
            var _ = UpdateRuntime();
        }


        private async Task UpdateRuntime()
        {
            using (await _runtimeLock.GetLock(nameof(UpdateRuntime)))
            {
                if (!_state.IsStopped())
                    return;

                if (_currentRuntimeId != _newestRuntimeId)
                {
                    _logger.Debug("Updating runtime...");

                    try
                    {
                        await UpdateRuntimeInternal();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to update package runtime");
                        ChangeState(PluginModelInfo.Types.PluginState.Broken, "Runtime update failure");
                    }
                }
            }
        }

        private async Task<bool> UpdateRuntimeInternal()
        {
            if (_currentRuntimeId == _newestRuntimeId)
                return _runtime != null;

            if (_runtime != null)
            {
                var detached = await RuntimeControlModel.DetachPlugin(_runtime, _id);
                if (!detached)
                {
                    BreakBot($"Can't detach from old runtime");
                    return false;
                }

                _currentRuntimeId = null;
                _runtime = null;
            }

            var pluginKey = _config.Key;
            var pkgId = pluginKey.PackageId;

            var runtimeId = _newestRuntimeId;
            if (string.IsNullOrEmpty(runtimeId))
            {
                _currentRuntimeId = runtimeId;
                BreakBot($"Algo package {pkgId} is not found");
                return false;
            }

            var runtime = await _server.GetRuntime(runtimeId);
            if (runtime == null)
            {
                BreakBot($"Runtime {runtimeId} is not found");
                return false;
            }

            _pluginInfo = await RuntimeControlModel.GetPluginInfo(runtime, pluginKey);
            if (_pluginInfo == null)
            {
                BreakBot($"Plugin '{pluginKey.DescriptorId}' is missing in Algo package '{pkgId}'");
                return false;
            }

            var attached = await RuntimeControlModel.AttachPlugin(runtime, _id, Self);
            if (!attached)
            {
                BreakBot($"Can't attach to new runtime");
                return false;
            }

            _currentRuntimeId = runtimeId;
            _runtime = runtime;

            if (_state == PluginModelInfo.Types.PluginState.Broken)
                ChangeState(PluginModelInfo.Types.PluginState.Stopped);

            _server.SendUpdate(PluginModelUpdate.Updated(_id, GetInfoCopy()));

            return true;
        }

        private void BreakBot(string reason)
        {
            ChangeState(PluginModelInfo.Types.PluginState.Broken, reason);
        }

        private void ChangeState(PluginModelInfo.Types.PluginState newState, string faultMsg = null)
        {
            if (string.IsNullOrWhiteSpace(faultMsg))
                _logger.Info($"State: {newState}", newState);
            else
                _logger.Error($"State: {newState} Error: {faultMsg}");
            _state = newState;
            _faultMsg = faultMsg;

            _server.SendUpdate(new PluginStateUpdate(_id, newState, faultMsg));
        }

        private ExecutorConfig CreateDefaultExecutorConfig()
        {
            var config = new ExecutorConfig { Id = _id, AccountId = _accId, IsLoggingEnabled = true, PluginConfig = Any.Pack(_config) };
            config.WorkingDirectory = _server.Env.GetPluginWorkingFolder(_id);
            config.LogDirectory = _server.Env.GetPluginLogsFolder(_id);
            config.InitPriorityInvokeStrategy();
            config.InitSlidingBuffering(4000);
            config.InitBarStrategy(Feed.Types.MarketSide.Bid);

            return config;
        }

        private async Task StartInternal()
        {
            _startTaskSrc = new TaskCompletionSource<bool>();
            if (_stopTaskSrc != null)
                await _stopTaskSrc.Task;

            try
            {
                ChangeState(PluginModelInfo.Types.PluginState.Starting);

                if (!await UpdateRuntimeInternal())
                {
                    _startTaskSrc.TrySetResult(false);
                    _startTaskSrc = null;
                    return;
                }

                await _server.SavedState.SetPluginRunning(_id, true);

                var config = CreateDefaultExecutorConfig();

                await RuntimeControlModel.StartExecutor(_runtime, config);

                _startTaskSrc.TrySetResult(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start plugin");
                _startTaskSrc.TrySetResult(false);
                ChangeState(PluginModelInfo.Types.PluginState.Faulted);
            }

            _startTaskSrc = null;
        }

        private async Task StopInternal()
        {
            _stopTaskSrc = new TaskCompletionSource<bool>();
            if (_startTaskSrc != null)
                await _startTaskSrc.Task;

            try
            {
                ChangeState(PluginModelInfo.Types.PluginState.Stopping);

                await _server.SavedState.SetPluginRunning(_id, false);

                await RuntimeControlModel.StopExecutor(_runtime, _id);

                _stopTaskSrc.TrySetResult(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop plugin");
                _stopTaskSrc.TrySetResult(false);
            }

            _stopTaskSrc = null;
        }


        private PluginModelInfo GetInfoCopy()
        {
            return new PluginModelInfo
            {
                InstanceId = _id,
                State = _state,
                AccountId = _accId,
                Descriptor_ = _pluginInfo?.Descriptor_,
                Config = _config,
                FaultMessage = _faultMsg,
            };
        }


        internal class StartCmd : Singleton<StartCmd> { }

        internal class StopCmd : Singleton<StopCmd> { }

        internal class UpdateConfigCmd
        {
            public PluginConfig NewConfig { get; }

            public UpdateConfigCmd(PluginConfig newConfig)
            {
                NewConfig = newConfig;
            }
        }

        internal class AttachLogsChannelCmd
        {
            public ChannelWriter<PluginLogRecord> LogSink { get; }

            public bool SendSnapshot { get; }

            public AttachLogsChannelCmd(ChannelWriter<PluginLogRecord> logSink, bool sendSnapshot)
            {
                LogSink = logSink;
                SendSnapshot = sendSnapshot;
            }
        }

        internal class AttachStatusChannelCmd
        {
            public ChannelWriter<PluginStatusUpdate> StatusSink { get; }

            public bool SendSnapshot { get; }

            public AttachStatusChannelCmd(ChannelWriter<PluginStatusUpdate> statusSink, bool sendSnapshot)
            {
                StatusSink = statusSink;
                SendSnapshot = sendSnapshot;
            }
        }

        internal class AttachOutputsChannelCmd
        {
            public ChannelWriter<DataSeriesUpdate> OutputSink { get; }

            public AttachOutputsChannelCmd(ChannelWriter<DataSeriesUpdate> outputSink)
            {
                OutputSink = outputSink;
            }
        }

        internal class RuntimeCrashedMsg : Singleton<RuntimeCrashedMsg> { }
    }
}
