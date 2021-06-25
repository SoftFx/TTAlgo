using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class PluginActor : Actor
    {
        private readonly AlgoServer _server;
        private readonly string _id, _accId;

        private PluginSavedState _savedState;
        private IAlgoLogger _logger;
        private PluginConfig _config;
        private PluginKey _pluginKey;

        private PluginModelInfo.Types.PluginState _state;
        private PkgRuntimeModel _runtime;
        private PluginInfo _pluginInfo;
        private string _faultMsg;
        private TaskCompletionSource<bool> _startTaskSrc, _stopTaskSrc, _updatePkgTaskSrc;
        private ExecutorModel _executor;


        private PluginActor(AlgoServer server, PluginSavedState savedState)
        {
            _server = server;
            _savedState = savedState;
            _id = savedState.Id;

            Receive<StartCmd>(Start);
            Receive<StopCmd>(Stop);
            Receive<UpdateConfigCmd>(UpdateConfig);
        }


        public static IActorRef Create(AlgoServer server, PluginSavedState savedState)
        {
            return ActorSystem.SpawnLocal(() => new PluginActor(server, savedState), $"{nameof(PluginActor)} ({savedState.Id})", new object());
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);

            _config = _savedState.UnpackConfig();

            var _ = UpdatePackage();
        }


        private Task Start(StartCmd cmd)
        {
            if (_state.IsRunning())
                return Task.CompletedTask;

            if (_startTaskSrc != null)
                return _startTaskSrc.Task;

            return StartInternal();
        }

        private Task Stop(StopCmd cmd)
        {
            if (_state.IsStopped())
                return Task.CompletedTask;

            if (_stopTaskSrc != null)
                return _stopTaskSrc.Task;

            return StopInternal();
        }

        private async Task UpdateConfig(UpdateConfigCmd cmd)
        {
            if (_state.IsRunning())
                throw Errors.PluginIsRunning(_id);

            _savedState.PackConfig(cmd.NewConfig);
            await _server.SavedState.UpdatePlugin(_savedState);
            _config = cmd.NewConfig;
        }


        private async Task<bool> UpdatePackage()
        {
            if (_updatePkgTaskSrc != null)
                return await _updatePkgTaskSrc.Task;

            _updatePkgTaskSrc = new TaskCompletionSource<bool>();

            var pluginKey = _config.Key;
            var pkgId = pluginKey.PackageId;

            _runtime = await _server.Runtimes.GetPkgRuntime(pkgId);
            if (_runtime == null)
            {
                BreakBot($"Algo package {pkgId} is not found");
                _updatePkgTaskSrc.SetResult(false);
                return false;
            }

            _pluginInfo = await _runtime.GetPluginInfo(pluginKey);
            if (_pluginInfo == null)
            {
                BreakBot($"Trade bot '{pluginKey.DescriptorId}' is missing in Algo package '{pkgId}'");
                _updatePkgTaskSrc.SetResult(false);
                return false;
            }

            if (_state == PluginModelInfo.Types.PluginState.Broken)
                ChangeState(PluginModelInfo.Types.PluginState.Stopped);

            _updatePkgTaskSrc.SetResult(true);
            _updatePkgTaskSrc = null;
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
            //StateChanged?.Invoke(new PluginStateUpdate { PluginId = _config.InstanceId, State = newState, FaultMessage = faultMsg });
        }

        private async Task StartInternal()
        {
            _startTaskSrc = new TaskCompletionSource<bool>();
            if (_stopTaskSrc != null)
                await _stopTaskSrc.Task;

            try
            {
                ChangeState(PluginModelInfo.Types.PluginState.Starting);

                if (!await UpdatePackage())
                {
                    _startTaskSrc.SetResult(false);
                    return;
                }

                await _server.SavedState.SetPluginRunning(_id, true);

                var config = new ExecutorConfig { AccountId = _accId, PluginConfig = Any.Pack(_config) };
                config.WorkingDirectory = _server.Env.GetPluginWorkingFolder(_id);
                config.InitPriorityInvokeStrategy();
                config.InitSlidingBuffering(4000);
                config.InitBarStrategy(Feed.Types.MarketSide.Bid);

                _executor = await _runtime.CreateExecutor(_id, config);

                await _executor.Start();

                _startTaskSrc.SetResult(true);
                ChangeState(PluginModelInfo.Types.PluginState.Running);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start plugin");
                _executor?.Dispose();
                _startTaskSrc.SetResult(false);
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

                await _executor.Stop();
                _executor.Dispose();

                _stopTaskSrc.SetResult(true);
                ChangeState(PluginModelInfo.Types.PluginState.Stopped);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop plugin");
                _stopTaskSrc.SetResult(false);
            }

            _stopTaskSrc = null;
        }


        internal class StartCmd { }

        internal class StopCmd { }

        internal class UpdateConfigCmd
        {
            public PluginConfig NewConfig { get; }

            public UpdateConfigCmd(PluginConfig newConfig)
            {
                NewConfig = newConfig;
            }
        }
    }
}
