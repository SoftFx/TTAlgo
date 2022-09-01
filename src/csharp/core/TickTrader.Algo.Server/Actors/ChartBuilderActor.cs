using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal class ChartBuilderActor : Actor
    {
        private readonly IActorRef _parent;
        private readonly AlgoServerPrivate _server;
        private readonly int _id;
        private readonly string _symbol;
        private readonly Feed.Types.Timeframe _timeframe;
        private readonly Feed.Types.MarketSide _marketSide;
        private readonly Dictionary<string, IActorRef> _indicators = new Dictionary<string, IActorRef>();

        private IAlgoLogger _logger;
        private bool _isStarted;


        public ChartBuilderActor(IActorRef parent, ChartInfo info, AlgoServerPrivate server)
        {
            _parent = parent;
            _server = server;
            _id = info.Id;
            _symbol = info.Symbol;
            _timeframe = info.Timeframe;
            _marketSide = info.MarketSide;

            Receive<ChartBuilderModel.StartCmd>(Start);
            Receive<ChartBuilderModel.StopCmd>(Stop);
            Receive<ChartBuilderModel.ClearCmd>(Clear);
            Receive<ChartHostProxy.AddIndicatorRequest>(AddIndicator);
            Receive<ChartHostProxy.UpdateIndicatorRequest>(UpdateIndicator);
            Receive<ChartHostProxy.RemoveIndicatorRequest>(RemoveIndicator);
            Receive<AlgoServerActor.PkgRuntimeUpdate>(OnPkgRuntimeUpdate);
        }


        public static IActorRef Create(IActorRef parent, ChartInfo info, AlgoServerPrivate server)
        {
            return ActorSystem.SpawnLocal(() => new ChartBuilderActor(parent, info, server), $"{nameof(ChartBuilderActor)} {info.Id}");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
        }


        private async Task Start(ChartBuilderModel.StartCmd cmd)
        {
            if (_isStarted)
                return;

            await StartAllIndicators();

            _isStarted = true;
        }

        private async Task Stop(ChartBuilderModel.StopCmd cmd)
        {
            if (!_isStarted)
                return;

            await StopAllIndicators();

            _isStarted = false;
        }

        private async Task Clear(ChartBuilderModel.ClearCmd cmd)
        {
            if (_isStarted)
                await StopAllIndicators();

            foreach (var pair in _indicators)
                await ShutdownIndicator(pair.Key, pair.Value);
        }

        private void OnPkgRuntimeUpdate(AlgoServerActor.PkgRuntimeUpdate update)
        {
            foreach (var plugin in _indicators.Values)
                plugin.Tell(update);
        }

        private async Task AddIndicator(ChartHostProxy.AddIndicatorRequest request)
        {
            var pluginId = request.Config.InstanceId;
            if (_indicators.ContainsKey(pluginId))
                throw new AlgoException("Indicator already exists");

            var plugin = (IActorRef)null;// PluginActor.Create();
            _indicators[pluginId] = plugin;

            if (_isStarted)
                await StartIndicator(plugin);
        }

        private async Task UpdateIndicator(ChartHostProxy.UpdateIndicatorRequest request)
        {
            var pluginId = request.Config.InstanceId;
            if (!_indicators.TryGetValue(pluginId, out var plugin))
                throw new AlgoException("Indicator not found");

            if (_isStarted)
                await StopIndicator(plugin);

            await plugin.Ask(new PluginActor.UpdateConfigCmd(request.Config));

            if (_isStarted)
                await StartIndicator(plugin);
        }

        private async Task RemoveIndicator(ChartHostProxy.RemoveIndicatorRequest request)
        {
            var pluginId = request.PluginId;
            if (!_indicators.TryGetValue(pluginId, out var plugin))
                throw new AlgoException("Indicator not found");

            _indicators.Remove(pluginId);

            if (_isStarted)
                await StopIndicator(plugin);

            await ShutdownIndicator(pluginId, plugin);
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
            foreach (var indicator in _indicators.Values)
                await StartIndicator(indicator);
        }

        private async Task StopAllIndicators()
        {
            foreach (var indicator in _indicators.Values)
                await StopIndicator(indicator);
        }
    }
}
