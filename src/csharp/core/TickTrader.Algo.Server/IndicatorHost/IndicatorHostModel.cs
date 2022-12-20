using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class IndicatorHostModel
    {
        private readonly IActorRef _actor;


        public IndicatorHostModel(IActorRef actor)
        {
            _actor = actor;
        }


        public Task Start() => _actor.Ask(StartCmd.Instance);

        public Task Stop() => _actor.Ask(StopCmd.Instance);

        public Task SetAccountProxy(IAccountProxy accProxy) => _actor.Ask(new SetAccountProxyCmd(accProxy));

        public Task<ChartHostProxy> CreateChart(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide,
            int barsCount = ChartBuilderActor.DefaultBarsCount)
                => _actor.Ask<ChartHostProxy>(new CreateChartRequest(symbol, timeframe, marketSide, barsCount));


        internal class StartCmd : Singleton<StartCmd> { }

        internal class StopCmd : Singleton<StopCmd> { }

        internal class ShutdownCmd : Singleton<ShutdownCmd> { }

        internal record SetAccountProxyCmd(IAccountProxy AccProxy);

        internal record CreateChartRequest(string Symbol, Feed.Types.Timeframe Timeframe, Feed.Types.MarketSide MarketSide, int BarsCount);

        internal record RemoveChartCmd(int ChartId);
    }
}
