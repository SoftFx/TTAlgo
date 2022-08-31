using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal class IndicatorHostActor : Actor
    {
        private readonly Dictionary<int, IActorRef> _charts = new Dictionary<int, IActorRef>();

        private IAccountProxy _accProxy;
        private bool _isStarted;
        private int _freeChartId;


        public IndicatorHostActor()
        {
            Receive<IndicatorHostModel.StartCmd>(Start);
            Receive<IndicatorHostModel.StopCmd>(Stop);
            Receive<IndicatorHostModel.SetAccountProxyCmd>(SetAccProxy);
            Receive<IndicatorHostModel.CreateChartRequest, ChartHostProxy>(CreateChart);
            Receive<IndicatorHostModel.RemoveChartCmd>(RemoveChart);
        }


        public static IActorRef Create()
        {
            return ActorSystem.SpawnLocal(() => new IndicatorHostActor(), $"{nameof(IndicatorHostActor)}");
        }


        private void Start(IndicatorHostModel.StartCmd cmd)
        {
            if (_isStarted)
                return;

            _isStarted = true;
        }

        private void Stop(IndicatorHostModel.StopCmd cmd)
        {
            if (!_isStarted)
                return;

            _isStarted = false;
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

            var chart = ChartBuilderActor.Create(info);
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

            try
            {
                await ChartBuilderModel.Stop(chart);
            }
            finally
            {
                _charts.Remove(chartId);
            }
        }
    }
}
