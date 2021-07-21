using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Server.Persistence
{
    public class ServerSavedState
    {
        public string Version { get; set; }

        public Dictionary<string, AccountSavedState> Accounts { get; set; } = new Dictionary<string, AccountSavedState>();

        public Dictionary<string, PluginSavedState> Plugins { get; set; } = new Dictionary<string, PluginSavedState>();


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
