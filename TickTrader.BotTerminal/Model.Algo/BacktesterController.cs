using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class BacktesterController
    {
        private readonly IActorRef _actor;


        public BacktesterController(IActorRef actor)
        {
            _actor = actor;
        }


        public Task Start(string configPath) => _actor.Ask(new StartBacktesterRequest { ConfigPath = configPath });

        public Task Stop() => _actor.Ask(new StopBacktesterRequest());

        public Task AwaitStop() => _actor.Ask(AwaitStopRequest.Instance);


        internal class AwaitStopRequest : Singleton<AwaitStopRequest> { }
    }
}
