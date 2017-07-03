using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class TradeBotModelConfig
    {
        public string InstanceId { get; set; }
        public bool Isolated { get; set; }
        public TradeBotPermissions Permissions { get; set; }
        public AccountKey Account { get; set; }
        public PluginKey Plugin { get; set; }
        public PluginConfig PluginConfig { get; set; }
    }

    public class TradeBotPermissions
    {
        public TradeBotPermissions()
        {
            TradeAllowed = true;
        }

        public bool TradeAllowed { get; set; }
    }
}
