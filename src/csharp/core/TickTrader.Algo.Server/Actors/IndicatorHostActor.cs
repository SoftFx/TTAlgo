using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal class IndicatorHostActor : Actor
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AlgoServerActor>();

        private readonly Dictionary<int, IActorRef> _charts = new Dictionary<int, IActorRef>();
        private readonly AlgoServerPrivate _server;

        private IAccountProxy _accProxy;
        private bool _isStarted;
        private int _freeChartId;


        public IndicatorHostActor(AlgoServerPrivate server)
        {
            _server = server;

            Receive<IndicatorHostModel.StartCmd>(Start);
            Receive<IndicatorHostModel.StopCmd>(Stop);
            Receive<IndicatorHostModel.ShutdownCmd>(Shutdown);
            Receive<IndicatorHostModel.SetAccountProxyCmd>(SetAccProxy);
            Receive<IndicatorHostModel.CreateChartRequest, ChartHostProxy>(CreateChart);
            Receive<IndicatorHostModel.RemoveChartCmd>(RemoveChart);
            Receive<AlgoServerActor.PkgRuntimeUpdate>(OnPkgRuntimeUpdate);
        }


        public static IActorRef Create(AlgoServerPrivate server)
        {
            return ActorSystem.SpawnLocal(() => new IndicatorHostActor(server), $"{nameof(IndicatorHostActor)}");
        }


        private void Start(IndicatorHostModel.StartCmd cmd)
        {
            if (_isStarted)
                return;

            foreach (var chart in _charts.Values)
                ChartBuilderModel.Start(chart);

            _isStarted = true;
        }

        private void Stop(IndicatorHostModel.StopCmd cmd)
        {
            if (!_isStarted)
                return;

            foreach (var chart in _charts.Values)
                ChartBuilderModel.Stop(chart);

            _isStarted = false;
        }

        private async Task Shutdown(IndicatorHostModel.ShutdownCmd cmd)
        {
            foreach (var pair in _charts)
                await ShutdownChart(pair.Key, pair.Value);
        }

        private void SetAccProxy(IndicatorHostModel.SetAccountProxyCmd cmd)
        {
            if (_isStarted)
                throw new AlgoException("Indicator host should be stopped before changin account");

            _accProxy = cmd.AccProxy;
        }

        private async Task<ChartHostProxy> CreateChart(IndicatorHostModel.CreateChartRequest request)
        {
            var info = new ChartInfo();
            info.Id = _freeChartId++;
            info.Symbol = request.Symbol;
            info.Timeframe = request.Timeframe;
            info.MarketSide = request.MarketSide;

            var chart = ChartBuilderActor.Create(Self, info, _server);
            if (_isStarted)
                await ChartBuilderModel.Start(chart);

            _charts[info.Id] = chart;

            return new ChartHostProxy(chart, Self, info);
        }

        private async Task RemoveChart(IndicatorHostModel.RemoveChartCmd cmd)
        {
            var chartId = cmd.ChartId;

            if (!_charts.TryGetValue(chartId, out var chart))
                throw new AlgoException($"Chart {chartId} not found");

            _charts.Remove(chartId);

            await ShutdownChart(chartId, chart);
        }

        private void OnPkgRuntimeUpdate(AlgoServerActor.PkgRuntimeUpdate update)
        {
            foreach (var chart in _charts.Values)
                chart.Tell(update);
        }


        private async Task ShutdownChart(int chartId, IActorRef chart)
        {
            await ChartBuilderModel.Clear(chart)
                .OnException(ex => _logger.Error(ex, $"Failed to clear chart {chartId}"));
            await ActorSystem.StopActor(chart)
                .OnException(ex => _logger.Error(ex, $"Failed to stop actor {chart.Name}"));
        }
    }
}
