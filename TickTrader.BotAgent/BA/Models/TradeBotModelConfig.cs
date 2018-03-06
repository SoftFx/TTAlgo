using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotAgent.BA.Models
{
    public class TradeBotModelConfig
    {
        public AccountKey Account { get; set; }
        public PluginKey Plugin { get; set; }
        public PluginConfig PluginConfig { get; set; }
    }
}
