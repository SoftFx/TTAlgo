using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class ServerStateModel
    {
        private readonly IActorRef _ref;


        public ServerStateModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task LoadSavedState(ServerSavedState state) => _ref.Ask(new ServerStateManager.LoadSavedStateCmd(state));

        public Task StopSaving() => _ref.Ask(new ServerStateManager.StopSavingCmd());

        public Task<ServerSavedState> GetSnapshot() => _ref.Ask<ServerSavedState>(new ServerStateManager.StateSnapshotRequest());

        public Task AddAccount(AccountSavedState acc) => _ref.Ask(new ServerStateManager.AddAccountCmd(acc));

        public Task UpdateAccount(AccountSavedState acc) => _ref.Ask(new ServerStateManager.UpdateAccountCmd(acc));

        public Task RemoveAccount(string accId) => _ref.Ask(new ServerStateManager.RemoveAccountCmd(accId));

        public Task AddPlugin(PluginSavedState plugin) => _ref.Ask(new ServerStateManager.AddPluginCmd(plugin));

        public Task UpdatePlugin(PluginSavedState plugin) => _ref.Ask(new ServerStateManager.UpdatePluginCmd(plugin));

        public Task RemovePlugin(string pluginId) => _ref.Ask(new ServerStateManager.RemovePluginCmd(pluginId));

        public Task SetPluginRunning(string pluginId, bool isRuning) => _ref.Ask(new ServerStateManager.SetPluginRunningCmd(pluginId, isRuning));
    }
}
