using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Server
{
    public class ChartBuilderModel
    {
        public static Task Start(IActorRef actor) => actor.Ask(StartCmd.Instance);

        public static Task Stop(IActorRef actor) => actor.Ask(StopCmd.Instance);


        internal class StartCmd : Singleton<StartCmd> { }

        internal class StopCmd : Singleton<StopCmd> { }
    }
}
