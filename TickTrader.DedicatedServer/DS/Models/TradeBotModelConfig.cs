using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class TradeBotModelConfig
    {
        public string InstanceId { get; set; }
        public bool Isolated { get; set; }
        public PluginPermissions Permissions { get; set; }
        public AccountKey Account { get; set; }
        public PluginKey Plugin { get; set; }
        public PluginConfig PluginConfig { get; set; }
    }
}
