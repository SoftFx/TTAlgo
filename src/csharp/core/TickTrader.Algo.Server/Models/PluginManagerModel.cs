using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    public class PluginManagerModel
    {
        private readonly IActorRef _ref;


        public PluginManagerModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task Shutdown() => _ref.Ask(new PluginManager.ShutdownCmd());

        public Task Restore() => _ref.Ask(new PluginManager.RestoreCmd());

        public Task<bool> PluginExists(string pluginId) => _ref.Ask<bool>(pluginId);

        public Task Add(AddPluginRequest request) => _ref.Ask(request);

        public Task UpdateConfig(ChangePluginConfigRequest request) => _ref.Ask(request);

        public Task Remove(RemovePluginRequest request) => _ref.Ask(request);

        public Task StartPlugin(StartPluginRequest request) => _ref.Ask(request);

        public Task StopPlugin(StopPluginRequest request) => _ref.Ask(request);

        public Task<PluginLogRecord[]> GetPluginLogs(PluginLogsRequest request) => _ref.Ask<PluginLogRecord[]>(request);

        public Task<string> GetPluginStatus(PluginStatusRequest request) => _ref.Ask<string>(request);
    }
}
