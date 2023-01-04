using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.IndicatorHost
{
    public class ChartBuilderModel
    {
        public static Task Start(IActorRef actor) => actor.Ask(StartCmd.Instance);

        public static Task Stop(IActorRef actor) => actor.Ask(StopCmd.Instance);

        public static Task Clear(IActorRef actor) => actor.Ask(ClearCmd.Instance);


        internal class StartCmd : Singleton<StartCmd> { }

        internal class StopCmd : Singleton<StopCmd> { }

        internal class ClearCmd : Singleton<ClearCmd> { }
    }
}
