using System.Collections.Generic;
using System.Runtime.Serialization;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.BotAgent.BA.Models
{
    [DataContract(Name = "account", Namespace = "")]
    public class ClientModel
    {
        [DataMember(Name = "bots")]
        private List<TradeBotModel> _bots = new List<TradeBotModel>();


        [DataMember(Name = "server")]
        public string Address { get; private set; }
        [DataMember(Name = "login")]
        public string Username { get; private set; }
        [DataMember(Name = "password")]
        public string Password { get; private set; }
        [DataMember(Name = "displayName")]
        public string DisplayName { get; private set; }

        internal void AddPluginsSavedStates(ServerSavedState state, string accId)
        {
            foreach (var plugin in _bots)
            {
                if (!plugin.OnDeserialized())
                    continue; // skip corrupted configs

                var pluginState = new PluginSavedState
                {
                    Id = plugin.Config.InstanceId,
                    AccountId = accId,
                    IsRunning = plugin.IsRunning,
                };

                pluginState.PackConfig(plugin.Config);

                state.Plugins.Add(pluginState.Id, pluginState);
            }
        }
    }
}
