using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

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


        public Task Start() => _ref.Ask(PluginActor.StartCmd.Instance);

        public Task Stop() => _ref.Ask(PluginActor.StopCmd.Instance);

        public Task UpdateConfig(PluginConfig newConfig) => _ref.Ask(new PluginActor.UpdateConfigCmd(newConfig));

        public Task<PluginLogRecord[]> GetLogs(PluginLogsRequest request) => _ref.Ask<PluginLogRecord[]>(request);

        public Task<string> GetStatus(PluginStatusRequest request) => _ref.Ask<string>(request);
    }
}
