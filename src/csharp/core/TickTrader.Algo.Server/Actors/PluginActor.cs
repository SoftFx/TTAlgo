using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Runtime;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class PluginActor : Actor
    {
        private readonly IPluginHost _host;
        private readonly string _id, _accId;
        private readonly ActorEventSource<PluginLogRecord> _logEventSrc = new ActorEventSource<PluginLogRecord>();
        private readonly ActorEventSource<PluginStatusUpdate> _statusEventSrc = new ActorEventSource<PluginStatusUpdate>();
        private readonly ActorEventSource<OutputSeriesUpdate> _outputEventSrc = new ActorEventSource<OutputSeriesUpdate>();
        private readonly ActorEventSource<object> _proxyDownlinkSrc = new ActorEventSource<object>();

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


        private PluginActor(IPluginHost host, PluginSavedState savedState)
        {
            _host = host;
            _savedState = savedState;
            _id = savedState.Id;
            _accId = savedState.AccountId;

            Receive<StartCmd>(Start);
            Receive<StopCmd>(Stop);
            Receive<ShutdownCmd>(Shutdown);
            Receive<UpdateConfigCmd>(UpdateConfig);
            Receive<AttachLogsChannelCmd>(AttachLogsChannel);
            Receive<AttachStatusChannelCmd>(AttachStatusChannel);
            Receive<AttachOutputsChannelCmd>(cmd => _outputEventSrc.Subscribe(cmd.OutputSink));
            Receive<PluginLogsRequest, PluginLogsResponse>(GetLogs);
            Receive<PluginStatusRequest, PluginStatusResponse>(GetStatus);

            Receive<PluginLogRecord>(OnLogUpdated);
            Receive<PluginStatusUpdate>(OnStatusUpdated);
            Receive<OutputSeriesUpdate>(update => _outputEventSrc.DispatchEvent(update));
            Receive<ExecutorStateUpdate>(OnExecutorStateUpdated);
            Receive<PluginExitedMsg>(OnExited);
            Receive<RuntimeControlModel.RuntimeCrashedMsg>(OnRuntimeCrashed);
            Receive<RuntimeControlModel.RuntimeInvalidMsg>(OnRuntimeInvalid);
            Receive<AlgoServerActor.PkgRuntimeUpdate>(OnPkgRuntimeUpdated);

            Receive<PluginListenerProxy.AttachProxyDownlinkCmd>(AttachProxyDownlink);
        }


        public static IActorRef Create(IPluginHost host, PluginSavedState savedState)
        {
            return ActorSystem.SpawnLocal(() => new PluginActor(host, savedState), $"{nameof(PluginActor)} ({savedState.Id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
            _logsCache = new MessageCache<PluginLogRecord>(100);
            _lastStatus = new PluginStatusUpdate { PluginId = _id, Message = string.Empty };

            _config = _savedState.UnpackConfig();

            _host.OnPluginUpdated(PluginModelUpdate.Added(_id, GetInfoCopy()));
            _runtimeLock = CreateLock();

            var _ = InitPkgRuntimeId();
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

        private async Task Shutdown(ShutdownCmd cmd)
        {
            using (await _runtimeLock.GetLock(nameof(ShutdownCmd)))
            {
                _logger.Debug("Shutting down...");

                if (_state.IsRunning())
                    await StopInternal();

                if (_runtime != null)
                {
                    var detached = await RuntimeControlModel.DetachPlugin(_runtime, _id);
                    if (!detached)
                        _logger.Error($"Failed to detach runtime");
                }
            }
        }

        private async Task UpdateConfig(UpdateConfigCmd cmd)
        {
            if (_state.IsRunning())
                throw Errors.PluginIsRunning(_id);

            _savedState.PackConfig(cmd.NewConfig);
            await _host.UpdateSavedState(_savedState);
            _config = cmd.NewConfig;

            var infoCopy = GetInfoCopy();
            _host.OnPluginUpdated(PluginModelUpdate.Updated(_id, infoCopy));
            //_proxyDownlinkSrc.DispatchEvent(infoCopy);
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

        private PluginLogsResponse GetLogs(PluginLogsRequest request)
        {
            var res = new PluginLogsResponse { PluginId = _id };
            res.Logs.AddRange(_logsCache.Where(u => u.TimeUtc > request.LastLogTimeUtc).Take(request.MaxCount).Select(r => new LogRecordInfo {TimeUtc = r.TimeUtc, Severity = r.Severity, Message = r.Message }));
            return res;
        }

        private PluginStatusResponse GetStatus(PluginStatusRequest request)
        {
            return new PluginStatusResponse { PluginId = _id, Status = _lastStatus.Message };
        }

        private void OnLogUpdated(PluginLogRecord log)
        {
            _logsCache.Add(log);
            _logEventSrc.DispatchEvent(log);
            _proxyDownlinkSrc.DispatchEvent(log);
            if (log.Severity == PluginLogRecord.Types.LogSeverity.Alert)
                _host.OnPluginAlert(_id, log);
        }

        private void OnStatusUpdated(PluginStatusUpdate update)
        {
            _lastStatus = update;
            _statusEventSrc.DispatchEvent(update);
            _proxyDownlinkSrc.DispatchEvent(update);
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
                _host.UpdateRunningState(_id, false).Forget();
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

            _host.UpdateRunningState(_id, false).Forget();
        }

        private void OnRuntimeCrashed(RuntimeControlModel.RuntimeCrashedMsg msg)
        {
            if (!_state.IsStopped())
            {
                _logger.Info("Runtime crashed during execution");

                ChangeState(PluginModelInfo.Types.PluginState.Faulted);

                _startTaskSrc?.TrySetResult(false);
                _stopTaskSrc?.TrySetResult(false);

                _host.OnGlobalAlert($"Process running plugin '{_id}' crashed");
                _host.UpdateRunningState(_id, false).Forget();
            }
        }

        private void OnRuntimeInvalid(RuntimeControlModel.RuntimeInvalidMsg msg)
        {
            if (_state.IsRunning())
            {
                _logger.Info("Runtime reported invalid state");

                _host.OnGlobalAlert($"Restart required for '{_id}'");
            }
        }

        private void OnPkgRuntimeUpdated(AlgoServerActor.PkgRuntimeUpdate update)
        {
            if (update.PkgId != _config.Key.PackageId)
                return;

            _newestRuntimeId = update.RuntimeId;
            var _ = UpdateRuntime();
        }

        private void AttachProxyDownlink(PluginListenerProxy.AttachProxyDownlinkCmd cmd)
        {
            var downlink = cmd.Sink;

            downlink.TryWrite(_lastStatus);
            foreach (var log in _logsCache)
            {
                downlink.TryWrite(log);
            }
            downlink.TryWrite(PluginListenerProxy.EndProxyInitMsg.Instance);

            _proxyDownlinkSrc.Subscribe(downlink);
        }


        private async Task InitPkgRuntimeId()
        {
            try
            {
                using (await _runtimeLock.GetLock(nameof(InitPkgRuntimeId)))
                {
                    _newestRuntimeId = await _host.GetPkgRuntimeId(_config.Key.PackageId);
                }

                var _ = UpdateRuntime(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to init package runtime id");
            }
        }

        private async Task UpdateRuntime(bool isInit = false)
        {
            using (await _runtimeLock.GetLock(nameof(UpdateRuntime)))
            {
                if (!_state.IsStopped())
                    return;

                if (isInit || _currentRuntimeId != _newestRuntimeId)
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
            var pluginKey = _config.Key;
            var pkgId = pluginKey.PackageId;

            if (_currentRuntimeId == _newestRuntimeId)
            {
                var res = _runtime != null;
                if (!res)
                    BreakBot($"Algo package {pkgId} is not found");
                return res;
            }

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

            var runtimeId = _newestRuntimeId;
            if (string.IsNullOrEmpty(runtimeId))
            {
                _currentRuntimeId = runtimeId;
                BreakBot($"Algo package {pkgId} is not found");
                return false;
            }

            var runtime = await _host.GetRuntime(runtimeId);
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

            var descriptor = _pluginInfo.Descriptor_;
            if (descriptor.Error != Metadata.Types.MetadataErrorCode.NoMetadataError)
            {
                BreakBot(AlgoMetadataException.CreateMessageDescription(descriptor.Error, null));
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

            var infoCopy = GetInfoCopy();
            _host.OnPluginUpdated(PluginModelUpdate.Updated(_id, infoCopy));
            //_proxyDownlinkSrc.DispatchEvent(infoCopy);

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

            var stateUpdate = new PluginStateUpdate(_id, newState, faultMsg);
            _host.OnPluginStateUpdated(stateUpdate);
            //_proxyDownlinkSrc.DispatchEvent(stateUpdate);

            if (_state.IsStopped())
                _ = UpdateRuntime();
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

                await _host.UpdateRunningState(_id, true);

                var config = _host.CreateExecutorConfig(_id, _accId, _config);

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

                await _host.UpdateRunningState(_id, false);

                await RuntimeControlModel.StopExecutor(_runtime, _id);

                _stopTaskSrc.TrySetResult(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop plugin");
                _stopTaskSrc.TrySetResult(false);
                ChangeState(PluginModelInfo.Types.PluginState.Faulted);
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

        internal class ShutdownCmd : Singleton<ShutdownCmd> { }

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
            public ChannelWriter<OutputSeriesUpdate> OutputSink { get; }

            public AttachOutputsChannelCmd(ChannelWriter<OutputSeriesUpdate> outputSink)
            {
                OutputSink = outputSink;
            }
        }
    }
}
