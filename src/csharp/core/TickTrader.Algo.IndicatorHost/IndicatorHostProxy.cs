using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;

namespace TickTrader.Algo.IndicatorHost
{
    public class IndicatorHostProxy
    {
        private IActorRef _algoHost, _indicatorHost;
        private bool _ownsAlgoHost;


        public IndicatorHostProxy() { }


        public async Task Init(IndicatorHostSettings settings, IActorRef algoHost = null)
        {
            _algoHost = algoHost;
            if (algoHost == null)
            {
                _ownsAlgoHost = true;
                settings.HostSettings.RuntimeSettings.WorkingDirectory ??= settings.DataFolder;
                _algoHost = AlgoHostActor.Create(settings.HostSettings);
            }

            _indicatorHost = IndicatorHostActor.Create(_algoHost, settings);
            await AlgoHostModel.AddConsumer(_algoHost, _indicatorHost);
            if (_ownsAlgoHost)
                await AlgoHostModel.Start(_algoHost);
        }

        public async Task Shutdown()
        {
            await IndicatorHostModel.Shutdown(_indicatorHost);
            if (_ownsAlgoHost)
                await AlgoHostModel.Stop(_algoHost);
        }

        public Task Start() => IndicatorHostModel.Start(_indicatorHost);

        public Task Stop() => IndicatorHostModel.Stop(_indicatorHost);

        public Task SetAccountProxy(IAccountProxy accProxy) => IndicatorHostModel.SetAccountProxy(_indicatorHost, accProxy);

        public Task<ChartHostProxy> CreateChart(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide,
            int barsCount = ChartBuilderActor.DefaultBarsCount) => IndicatorHostModel.CreateChart(_indicatorHost, symbol, timeframe, marketSide, barsCount);
    }
}
