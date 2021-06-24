using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Server.Persistence
{
    internal class ServerSavedState
    {
        public string Version { get; set; }

        public Dictionary<string, AccountSavedState> Accounts { get; private set; } = new Dictionary<string, AccountSavedState>();

        public Dictionary<string, PluginSavedState> Plugins { get; private set; } = new Dictionary<string, PluginSavedState>();


        public ServerSavedState Clone()
        {
            return new ServerSavedState
            {
                Version = Version,
                Plugins = Plugins.ToDictionary(p => p.Key, p => p.Value.Clone()),
                Accounts = Accounts.ToDictionary(p => p.Key, p => p.Value.Clone()),
            };
        }
    }
}
