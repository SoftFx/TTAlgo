using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotAgent.BA.Entities
{
    public class TradeBotConfig
    {
        //public AccountKey Account { get; set; }
        public PluginKey Plugin { get; set; }
        public PluginConfig PluginConfig { get; set; }
    }
}
