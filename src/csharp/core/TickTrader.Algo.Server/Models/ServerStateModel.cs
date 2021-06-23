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


        public Task AddAccount(AccountState acc) => _ref.Ask(new ServerStateManager.AddAccountCmd(acc));

        public Task UpdateAccount(AccountState acc) => _ref.Ask(new ServerStateManager.UpdateAccountCmd(acc));

        public Task RemoveAccount(string accId) => _ref.Ask(new ServerStateManager.RemoveAccountCmd(accId));

        public Task AddPlugin(PluginState plugin) => _ref.Ask(new ServerStateManager.AddPluginCmd(plugin));

        public Task UpdatePlugin(PluginState plugin) => _ref.Ask(new ServerStateManager.UpdatePluginCmd(plugin));

        public Task RemovePlugin(string pluginId) => _ref.Ask(new ServerStateManager.RemovePluginCmd(pluginId));

        public Task SetPluginRunning(string pluginId, bool isRuning) => _ref.Ask(new ServerStateManager.SetPluginRunningCmd(pluginId, isRuning));
    }
}
