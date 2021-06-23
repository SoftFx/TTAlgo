using System.Collections.Generic;

namespace TickTrader.Algo.Server.Persistence
{
    internal class ServerState
    {
        public Dictionary<string, PluginState> Plugins { get; } = new Dictionary<string, PluginState>();

        public Dictionary<string, AccountState> Accounts { get; } = new Dictionary<string, AccountState>();
    }
}
