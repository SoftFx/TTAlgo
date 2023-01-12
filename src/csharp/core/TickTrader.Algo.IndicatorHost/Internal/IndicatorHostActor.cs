using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;

namespace TickTrader.Algo.IndicatorHost
{
    internal class IndicatorHostActor : Actor
    {
        public const string AccId = "acc/indicators";

        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<IndicatorHostActor>();

        private readonly Dictionary<int, IActorRef> _charts = new();
        private readonly ActorEventSource<object> _proxyDownlinkSrc = new();
        private readonly TimeKeyGenerator _timeGen = new();
        private readonly IActorRef _algoHost;
        private readonly IndicatorHostSettings _settings;

        private EnvService _env;
        private IAccountProxy _accProxy;
        private AccountRpcController _accountRpcController;
        private bool _isStarted;
        private int _freeChartId;


        private IndicatorHostActor(IActorRef algoHost, IndicatorHostSettings settings)
        {
            _algoHost = algoHost;
            _settings = settings;

            Receive<IndicatorHostModel.StartCmd>(Start);
            Receive<IndicatorHostModel.StopCmd>(Stop);
            Receive<IndicatorHostModel.ShutdownCmd>(Shutdown);
            Receive<IndicatorHostModel.SetAccountProxyCmd>(SetAccProxy);
            Receive<IndicatorHostModel.CreateChartRequest, ChartHostProxy>(CreateChart);
            Receive<IndicatorHostModel.RemoveChartCmd>(RemoveChart);

            Receive<PackageUpdate>(upd => _proxyDownlinkSrc.DispatchEvent(upd));
            Receive<PackageStateUpdate>(upd => _proxyDownlinkSrc.DispatchEvent(upd));
            Receive<RuntimeControlModel.PkgRuntimeUpdateMsg>(OnPkgRuntimeUpdate);
            Receive<RuntimeServerModel.AccountControlRequest, IActorRef>(GetAccountControlInternal);

            Receive<AccountRpcModel.AttachSessionCmd, AccountRpcHandler>(AttachSession);
            Receive<AccountRpcModel.DetachSessionCmd>(DetachSession);

            Receive<IndicatorHostProxy.AttachDownlinkCmd>(cmd => _proxyDownlinkSrc.Subscribe(cmd.Sink));

            Receive<ChartBuilderActor.PluginAlertMsg>(OnPluginAlertMsg);
            Receive<ChartBuilderActor.GlobalAlertMsg>(OnGlobalAlertMsg);
            Receive<IndicatorLogRecord>(l => _proxyDownlinkSrc.DispatchEvent(l));
        }


        public static IActorRef Create(IActorRef algoHost, IndicatorHostSettings settigns)
        {
            return ActorSystem.SpawnLocal(() => new IndicatorHostActor(algoHost, settigns), $"{nameof(IndicatorHostActor)}");
        }


        protected override void ActorInit(object initMsg)
        {
            _env = new EnvService(_settings.DataFolder);
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

            var chart = ChartBuilderActor.Create(Self, info, _algoHost, _env);
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

        private AccountRpcHandler AttachSession(AccountRpcModel.AttachSessionCmd cmd)
        {
            var accState = _isStarted ? Domain.Account.Types.ConnectionState.Online : Domain.Account.Types.ConnectionState.Offline;
            return _accountRpcController.AttachSession(cmd.Session, accState);
        }

        private void DetachSession(AccountRpcModel.DetachSessionCmd cmd)
        {
            _accountRpcController.DetachSession(cmd.SessionId);
        }

        private IActorRef GetAccountControlInternal(RuntimeServerModel.AccountControlRequest request) => request.Id == AccId ? Self : null;


        private async Task ShutdownChart(int chartId, IActorRef chart)
        {
            await ChartBuilderModel.Clear(chart)
                .OnException(ex => _logger.Error(ex, $"Failed to clear chart {chartId}"));
            await ActorSystem.StopActor(chart)
                .OnException(ex => _logger.Error(ex, $"Failed to stop actor {chart.Name}"));
        }

        private void OnPluginAlertMsg(ChartBuilderActor.PluginAlertMsg msg)
        {
            var log = msg.LogRecord;
            if (log == null)
                return;

            var pluginId = msg.Id;
            var severity = msg.LogRecord.Severity;
            if (severity != PluginLogRecord.Types.LogSeverity.Alert)
            {
                _logger.Error($"Received log with severity '{severity}' from plugin '{pluginId}'");
                return;
            }

            var alert = new AlertRecordInfo
            {
                PluginId = pluginId,
                Message = log.Message,
                TimeUtc = log.TimeUtc,
                Type = AlertRecordInfo.Types.AlertType.Plugin,
            };

            _proxyDownlinkSrc.DispatchEvent(alert);
        }

        private void OnGlobalAlertMsg(ChartBuilderActor.GlobalAlertMsg msg)
        {
            var alert = new AlertRecordInfo
            {
                PluginId = "<IndicatorHost>",
                Message = msg.Message,
                // In concurrent scenarios we can't guarantee time sequence to be ascending when we receive messages from many thread.
                // Therefore we have to assign our own time. Plugin alerts time is assigned on plugin thread within log time sequence
                TimeUtc = _timeGen.NextKey(DateTime.UtcNow),
                Type = AlertRecordInfo.Types.AlertType.Server,
            };

            _proxyDownlinkSrc.DispatchEvent(alert);
        }
    }
}
