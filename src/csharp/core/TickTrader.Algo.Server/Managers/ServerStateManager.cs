using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class ServerStateManager : Actor
    {
        private readonly ServerState _state;


        public ServerStateManager()
        {
            Receive<AddAccountCmd, object>(AddAccount);
            Receive<RemoveAccountCmd, object>(RemoveAccount);
            Receive<UpdateAccountCmd, object>(UpdateAccount);
            Receive<AddPluginCmd, object>(AddPlugin);
            Receive<UpdatePluginCmd, object>(UpdatePlugin);
            Receive<RemovePluginCmd, object>(RemovePlugin);
        }


        public static IActorRef Create()
        {
            return ActorSystem.SpawnLocal<ServerStateManager>(nameof(ServerStateManager));
        }


        private object AddAccount(AddAccountCmd cmd)
        {
            var accId = cmd.Account.Id;
            if (_state.Accounts.ContainsKey(accId))
                return Errors.DuplicateAccount(accId);

            _state.Accounts.Add(accId, cmd.Account);

            return null;
        }

        private object UpdateAccount(UpdateAccountCmd cmd)
        {
            var accId = cmd.Account.Id;
            if (!_state.Accounts.Remove(accId))
                return Errors.AccountNotFound(accId);

            _state.Accounts[accId] = cmd.Account;

            return null;
        }

        private object RemoveAccount(RemoveAccountCmd cmd)
        {
            var accId = cmd.Id;
            if (!_state.Accounts.Remove(accId))
                return Errors.AccountNotFound(accId);

            return null;
        }

        private object AddPlugin(AddPluginCmd cmd)
        {
            var id = cmd.Plugin.Id;
            if (_state.Plugins.ContainsKey(id))
                return Errors.DuplicatePlugin(id);

            _state.Plugins.Add(id, cmd.Plugin);

            return null;
        }

        private object UpdatePlugin(UpdatePluginCmd cmd)
        {
            var id = cmd.Plugin.Id;
            if (!_state.Plugins.ContainsKey(id))
                return Errors.PluginNotFound(id);

            _state.Plugins[id] = cmd.Plugin;

            return null;
        }

        private object RemovePlugin(RemovePluginCmd cmd)
        {
            var id = cmd.Id;
            if (!_state.Plugins.Remove(id))
                return Errors.PluginNotFound(id);

            return null;
        }

        private object SetPluginRunning(SetPluginRunningCmd cmd)
        {
            var id = cmd.Id;
            if (!_state.Plugins.TryGetValue(id, out var plugin))
                return Errors.PluginNotFound(id);

            plugin.IsRunning = cmd.IsRunning;

            return null;
        }


        internal class AddAccountCmd
        {
            public AccountState Account { get; }

            public AddAccountCmd(AccountState account)
            {
                Account = account;
            }
        }

        internal class UpdateAccountCmd
        {
            public AccountState Account { get; }

            public UpdateAccountCmd(AccountState account)
            {
                Account = account;
            }
        }

        internal class RemoveAccountCmd
        {
            public string Id { get; }

            public RemoveAccountCmd(string id)
            {
                Id = id;
            }
        }

        internal class AddPluginCmd
        {
            public PluginState Plugin { get; }

            public AddPluginCmd(PluginState plugin)
            {
                Plugin = plugin;
            }
        }

        internal class UpdatePluginCmd
        {
            public PluginState Plugin { get; }

            public UpdatePluginCmd(PluginState plugin)
            {
                Plugin = plugin;
            }
        }

        internal class RemovePluginCmd
        {
            public string Id { get; }

            public RemovePluginCmd(string id)
            {
                Id = id;
            }
        }

        internal class SetPluginRunningCmd
        {
            public string Id { get; }

            public bool IsRunning { get; }

            public SetPluginRunningCmd(string id, bool isRunning)
            {
                Id = id;
                IsRunning = isRunning;
            }
        }
    }
}
