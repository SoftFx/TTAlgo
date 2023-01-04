using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.IndicatorHost
{
    public static class IndicatorHostModel
    {
        public static Task Start(IActorRef actor) => actor.Ask(StartCmd.Instance);

        public static Task Stop(IActorRef actor) => actor.Ask(StopCmd.Instance);

        public static Task Shutdown(IActorRef actor) => actor.Ask(ShutdownCmd.Instance);

        public static Task SetAccountProxy(IActorRef actor, IAccountProxy accProxy) => actor.Ask(new SetAccountProxyCmd(accProxy));

        public static Task<ChartHostProxy> CreateChart(IActorRef actor, string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide,
            int barsCount = ChartBuilderActor.DefaultBarsCount)
                => actor.Ask<ChartHostProxy>(new CreateChartRequest(symbol, timeframe, marketSide, barsCount));


        internal class StartCmd : Singleton<StartCmd> { }

        internal class StopCmd : Singleton<StopCmd> { }

        internal class ShutdownCmd : Singleton<ShutdownCmd> { }

        internal record SetAccountProxyCmd(IAccountProxy AccProxy);

        internal record CreateChartRequest(string Symbol, Feed.Types.Timeframe Timeframe, Feed.Types.MarketSide MarketSide, int BarsCount);

        internal record RemoveChartCmd(int ChartId);
    }
}
