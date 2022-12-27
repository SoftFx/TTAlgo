using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal class IndicatorHostActor : Actor
    {
        public const string AccId = "acc/indicators";

        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AlgoServerActor>();

        private readonly Dictionary<int, IActorRef> _charts = new();
        private readonly AlgoServerPrivate _server;

        private IAccountProxy _accProxy;
        private AccountRpcController _accountRpcController;
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
            Receive<RuntimeControlModel.PkgRuntimeUpdateMsg>(OnPkgRuntimeUpdate);
            Receive<AccountRpcController.AttachSessionCmd, AccountRpcHandler>(AttachSession);
            Receive<AccountRpcController.DetachSessionCmd>(DetachSession);
        }


        public static IActorRef Create(AlgoServerPrivate server)
        {
            return ActorSystem.SpawnLocal(() => new IndicatorHostActor(server), $"{nameof(IndicatorHostActor)}");
        }


        protected override void ActorInit(object initMsg)
        {
            _accountRpcController = new AccountRpcController(_logger, AccId);
        }


        private async Task Start(IndicatorHostModel.StartCmd cmd)
        {
            if (_isStarted)
                return;

            _isStarted = true;

            foreach (var chart in _charts.Values)
                await ChartBuilderModel.Start(chart);
        }

        private async Task Stop(IndicatorHostModel.StopCmd cmd)
        {
            if (!_isStarted)
                return;

            _isStarted = false;

            foreach (var chart in _charts.Values)
                await ChartBuilderModel.Stop(chart);
        }

        private async Task Shutdown(IndicatorHostModel.ShutdownCmd cmd)
        {
            _isStarted = false;

            foreach (var pair in _charts)
                await ShutdownChart(pair.Key, pair.Value);

            _charts.Clear();
        }

        private void SetAccProxy(IndicatorHostModel.SetAccountProxyCmd cmd)
        {
            if (_isStarted)
                throw new AlgoException("Indicator host should be stopped before changin account");

            _accProxy = cmd.AccProxy;
            _accountRpcController.SetAccountProxy(cmd.AccProxy);
        }

        private async Task<ChartHostProxy> CreateChart(IndicatorHostModel.CreateChartRequest request)
        {
            var info = new ChartInfo
            {
                Id = _freeChartId++,
                Symbol = request.Symbol,
                Timeframe = request.Timeframe,
                MarketSide = request.MarketSide,
                Boundaries = new ChartBoundaries { BarsCount = request.BarsCount }
            };

            var chart = ChartBuilderActor.Create(Self, info, _server);
            if (_isStarted)
                await ChartBuilderModel.Start(chart);

            _charts[info.Id] = chart;

            var res = new ChartHostProxy(chart, Self, info);
            await res.Init();
            return res;
        }

        private async Task RemoveChart(IndicatorHostModel.RemoveChartCmd cmd)
        {
            var chartId = cmd.ChartId;

            if (!_charts.TryGetValue(chartId, out var chart))
                throw new AlgoException($"Chart {chartId} not found");

            _charts.Remove(chartId);

            await ShutdownChart(chartId, chart);
        }

        private void OnPkgRuntimeUpdate(RuntimeControlModel.PkgRuntimeUpdateMsg update)
        {
            foreach (var chart in _charts.Values)
                chart.Tell(update);
        }

        private AccountRpcHandler AttachSession(AccountRpcController.AttachSessionCmd cmd)
        {
            var accState = _isStarted ? Domain.Account.Types.ConnectionState.Online : Domain.Account.Types.ConnectionState.Offline;
            return _accountRpcController.AttachSession(cmd.Session, accState);
        }

        private void DetachSession(AccountRpcController.DetachSessionCmd cmd)
        {
            _accountRpcController.DetachSession(cmd.SessionId);
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
