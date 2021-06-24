using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class PluginModel
    {
        private readonly IActorRef _ref;


        public string Id { get; }


        public PluginModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task Start() => _ref.Ask(new PluginActor.StartCmd());

        public Task Stop() => _ref.Ask(new PluginActor.StopCmd());

        public Task UpdateConfig(PluginConfig newConfig) => _ref.Ask(new PluginActor.UpdateConfigCmd(newConfig));
    }
}
