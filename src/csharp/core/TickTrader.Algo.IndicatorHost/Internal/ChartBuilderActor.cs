﻿using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.IndicatorHost
{
    internal class ChartBuilderActor : Actor, IPluginHost
    {
        public const int DefaultBarsCount = 512;

        private readonly IActorRef _parent, _algoHost;
        private readonly EnvService _env;
        private readonly ChartInfo _info;
        private readonly Dictionary<string, IndicatorModel> _indicators = new();
        private readonly ActorEventSource<object> _proxyDownlinkSrc = new();

        private IAlgoLogger _logger;
        private bool _isStarted;
        private string _chartDisplayName;


        public ChartBuilderActor(IActorRef parent, ChartInfo info, IActorRef algoHost, EnvService env)
        {
            _parent = parent;
            _info = info;
            _algoHost = algoHost;
            _env = env;

            Receive<ChartBuilderModel.StartCmd>(Start);
            Receive<ChartBuilderModel.StopCmd>(Stop);
            Receive<ChartBuilderModel.ClearCmd>(Clear);
            Receive<RuntimeControlModel.PkgRuntimeUpdateMsg>(OnPkgRuntimeUpdate);
            Receive<ChartHostProxy.AddIndicatorRequest>(AddIndicator);
            Receive<ChartHostProxy.UpdateIndicatorRequest>(UpdateIndicator);
            Receive<ChartHostProxy.RemoveIndicatorRequest>(RemoveIndicator);
            Receive<ChartHostProxy.AttachDownlinkCmd>(AttachProxyDownlink);
            Receive<ChartHostProxy.ChangeTimeframeCmd>(ChangeTimeframe);
            Receive<ChartHostProxy.ChangeBoundariesCmd>(ChangeBoundaries);

            Receive<RunningStateChanged>(OnRunningStateChanged);
            Receive<PluginModelUpdate>(OnPluginUpdated);
            Receive<PluginStateUpdate>(OnPluginStateUpdated);
        }


        public static IActorRef Create(IActorRef parent, ChartInfo info, IActorRef algoHost, EnvService env)
        {
            return ActorSystem.SpawnLocal(() => new ChartBuilderActor(parent, info, algoHost, env), $"{nameof(ChartBuilderActor)} {info.Id}");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
            InitChartDisplayName();
        }


        private async Task Start(ChartBuilderModel.StartCmd cmd)
        {
            if (_isStarted)
                return;

            _isStarted = true;

            await StartAllIndicators();
        }

        private async Task Stop(ChartBuilderModel.StopCmd cmd)
        {
            if (!_isStarted)
                return;

            _isStarted = false;

            await StopAllIndicators();
        }

        private async Task Clear(ChartBuilderModel.ClearCmd cmd)
        {
            if (_isStarted)
                await StopAllIndicators();

            foreach (var pair in _indicators)
                await ShutdownIndicator(pair.Key, pair.Value.PluginRef);
        }

        private void OnPkgRuntimeUpdate(RuntimeControlModel.PkgRuntimeUpdateMsg update)
        {
            foreach (var indicator in _indicators.Values)
                indicator.PluginRef.Tell(update);
        }

        private async Task ChangeTimeframe(ChartHostProxy.ChangeTimeframeCmd cmd)
        {
            _info.Timeframe = cmd.Timeframe;

            if (!_isStarted)
            {
                InitChartDisplayName();
                return;
            }

            await StopAllIndicators();

            InitChartDisplayName();
            if (!_isStarted)
                return;

            await StartAllIndicators();
        }

        private async Task ChangeBoundaries(ChartHostProxy.ChangeBoundariesCmd cmd)
        {
            _info.Boundaries = new ChartBoundaries { BarsCount = cmd.BarsCount ?? DefaultBarsCount };

            if (!_isStarted)
                return;

            await StopAllIndicators();

            if (!_isStarted)
                return;

            await StartAllIndicators();
        }

        private async Task AddIndicator(ChartHostProxy.AddIndicatorRequest request)
        {
            var pluginId = request.Config.InstanceId;
            if (_indicators.ContainsKey(pluginId))
                throw new AlgoException("Indicator already exists");

            var savedState = new PluginSavedState
            {
                Id = pluginId,
                AccountId = IndicatorHostActor.AccId,
                IsRunning = false,
            };
            savedState.PackConfig(request.Config);

            var plugin = PluginActor.Create(this, savedState);
            var indicator = new IndicatorModel(plugin);
            _indicators[pluginId] = indicator;
            await indicator.Init();
            _ = indicator.OutputChannel.Consume(update => _proxyDownlinkSrc.DispatchEvent(new ChartHostProxy.OutputDataUpdatedMsg(pluginId, update)));
            _ = indicator.LogChannel.Consume(log => _parent.Tell(new IndicatorLogRecord(pluginId, _info.Id, _chartDisplayName, log)));
            _ = indicator.DrawableChannel.Consume(update => _proxyDownlinkSrc.DispatchEvent(new ChartHostProxy.DrawableObjectUpdatedMsg(pluginId, update)));

            if (_isStarted)
                await StartIndicator(plugin);
        }

        private async Task UpdateIndicator(ChartHostProxy.UpdateIndicatorRequest request)
        {
            var pluginId = request.Config.InstanceId;
            if (!_indicators.TryGetValue(pluginId, out var indicator))
                throw new AlgoException("Indicator not found");

            if (_isStarted)
                await StopIndicator(indicator.PluginRef);

            await indicator.PluginRef.Ask(new PluginActor.UpdateConfigCmd(request.Config));

            if (_isStarted)
                await StartIndicator(indicator.PluginRef);
        }

        private async Task RemoveIndicator(ChartHostProxy.RemoveIndicatorRequest request)
        {
            var pluginId = request.PluginId;
            if (!_indicators.TryGetValue(pluginId, out var indicator))
                throw new AlgoException("Indicator not found");

            _indicators.Remove(pluginId);

            if (_isStarted)
                await StopIndicator(indicator.PluginRef);

            indicator.Deinit();
            _proxyDownlinkSrc.DispatchEvent(PluginModelUpdate.Removed(pluginId));

            await ShutdownIndicator(pluginId, indicator.PluginRef);
        }

        private void AttachProxyDownlink(ChartHostProxy.AttachDownlinkCmd cmd)
        {
            var downlink = cmd.Sink;

            _proxyDownlinkSrc.Subscribe(downlink);  
        }

        private void OnRunningStateChanged(RunningStateChanged stateChanged)
        {
            if (_indicators.TryGetValue(stateChanged.PluginId, out var indicator))
            {
                indicator.IsRunning = stateChanged.IsRunning;
            }
        }

        private void OnPluginUpdated(PluginModelUpdate update)
        {
            if (_indicators.TryGetValue(update.Id, out var indicator))
            {
                indicator.OnModelUpdate(update);
            }
            _proxyDownlinkSrc.DispatchEvent(update);
        }

        private void OnPluginStateUpdated(PluginStateUpdate update)
        {
            if (_indicators.TryGetValue(update.Id, out var indicator))
            {
                indicator.OnStateUpdate(update);
            }
            _proxyDownlinkSrc.DispatchEvent(update);
        }


        private async Task StartIndicator(IActorRef plugin)
        {
            await plugin.Ask(PluginActor.StartCmd.Instance);
        }

        private async Task StopIndicator(IActorRef plugin)
        {
            await plugin.Ask(PluginActor.StopCmd.Instance);
        }

        private async Task ShutdownIndicator(string id, IActorRef plugin)
        {
            await plugin.Ask(PluginActor.ShutdownCmd.Instance)
                .OnException(ex => _logger.Error(ex, $"Failed to shutdown indicator {id}"));

            await ActorSystem.StopActor(plugin)
                .OnException(ex => _logger.Error(ex, $"Failed to stop actor {plugin.Name}"));
        }

        private async Task StartAllIndicators()
        {
            //await Task.WhenAll(_indicators.Values.Select(ind => StartIndicator(ind.PluginRef)));
            foreach (var indicator in _indicators.Values)
                await StartIndicator(indicator.PluginRef);
        }

        private async Task StopAllIndicators()
        {
            //await Task.WhenAll(_indicators.Values.Select(ind => StopIndicator(ind.PluginRef)));
            foreach (var indicator in _indicators.Values)
                await StopIndicator(indicator.PluginRef);
        }

        private void InitChartDisplayName()
        {
            _chartDisplayName = $"{_info.Symbol}, {_info.Timeframe}";
        }


        #region IPluginHost implementation

        Task IPluginHost.UpdateRunningState(string pluginId, bool isRunning)
        {
            Self.Tell(new RunningStateChanged(pluginId, isRunning));

            return Task.CompletedTask;
        }

        Task IPluginHost.UpdateSavedState(PluginSavedState savedState) => Task.CompletedTask;

        void IPluginHost.OnPluginUpdated(PluginModelUpdate update) => Self.Tell(update);

        void IPluginHost.OnPluginStateUpdated(PluginStateUpdate update) => Self.Tell(update);

        void IPluginHost.OnPluginAlert(string pluginId, PluginLogRecord record) => _parent.Tell(new PluginAlertMsg(pluginId, record));

        void IPluginHost.OnGlobalAlert(string msg) => _parent.Tell(new GlobalAlertMsg(msg));

        Task<string> IPluginHost.GetPkgRuntimeId(string pkgId) => RuntimeServerModel.GetPkgRuntimeId(_algoHost, pkgId);

        Task<IActorRef> IPluginHost.GetRuntime(string runtimeId) => RuntimeServerModel.GetRuntime(_algoHost, runtimeId);

        ExecutorConfig IPluginHost.CreateExecutorConfig(string pluginId, string accId, PluginConfig pluginConfig)
        {
            pluginConfig = pluginConfig.Clone();
            pluginConfig.MainSymbol.Name = _info.Symbol;
            pluginConfig.MainSymbol.Origin = SymbolConfig.Types.SymbolOrigin.Online;
            pluginConfig.Timeframe = _info.Timeframe;

            var config = new ExecutorConfig { Id = pluginId, AccountId = accId, IsLoggingEnabled = true, SaveLogsOnDisk = false };
            config.SetPluginConfig(pluginConfig);
            config.WorkingDirectory = _env.AlgoWorkingFolder;
            config.InitPriorityInvokeStrategy();
            config.InitSlidingBuffering(_info.Boundaries?.BarsCount ?? DefaultBarsCount);
            config.InitBarStrategy(_info.MarketSide);

            return config;
        }

        #endregion IPluginHost implementation


        internal record PluginAlertMsg(string Id, PluginLogRecord LogRecord);

        internal record GlobalAlertMsg(string Message);

        private record RunningStateChanged(string PluginId, bool IsRunning);


        private class IndicatorModel
        {
            public IActorRef PluginRef { get; }

            public bool IsRunning { get; set; }

            public PluginModelUpdate LastUpdate { get; set; }

            public PluginModelInfo Info { get; set; }

            public Channel<OutputSeriesUpdate> OutputChannel { get; }

            public Channel<PluginLogRecord> LogChannel { get; }

            public Channel<DrawableCollectionUpdate> DrawableChannel { get; }


            public IndicatorModel(IActorRef plugin)
            {
                PluginRef = plugin;

                OutputChannel = DefaultChannelFactory.CreateForOneToOne<OutputSeriesUpdate>();
                LogChannel = DefaultChannelFactory.CreateForOneToOne<PluginLogRecord>();
                DrawableChannel = DefaultChannelFactory.CreateForOneToOne<DrawableCollectionUpdate>();
            }


            public async Task Init()
            {
                await PluginRef.Ask(new PluginActor.AttachOutputsChannelCmd(OutputChannel));
                await PluginRef.Ask(new PluginActor.AttachLogsChannelCmd(LogChannel, false));
                await PluginRef.Ask(new PluginActor.AttachDrawableChannelCmd(DrawableChannel));
            }

            public void Deinit()
            {
                OutputChannel.Writer.TryComplete();
                LogChannel.Writer.TryComplete();
                DrawableChannel.Writer.TryComplete();
            }

            public void OnModelUpdate(PluginModelUpdate update)
            {
                LastUpdate = update;
                Info = update.Plugin;
            }

            public void OnStateUpdate(PluginStateUpdate update)
            {
                if (Info != null)
                {
                    Info.State = update.State;
                    Info.FaultMessage = update.FaultMessage;
                }
            }
        }
    }
}
