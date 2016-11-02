using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace BotCommon
{
    public class TradeAsync
    {
        readonly TradeBot bot;
        Dictionary<string, OrderKeeper> dict = new Dictionary<string, OrderKeeper>();
        OrderKeeperSettings okSettings { get; set; }

        public TradeAsync(TradeBot bot, OrderKeeperSettings orderKeeperSettings = OrderKeeperSettings.None)
        {
            this.bot = bot;
            this.okSettings = orderKeeperSettings;
        }

        public void SetLimitOrderAsync(string symbol, OrderSide side, double volume, double price, 
            string comment, string subBotTag)
        {
            string key = GetKey(symbol, side, subBotTag);
            if (!dict.ContainsKey(key))
                dict.Add(key, new OrderKeeper(bot, bot.Symbols[symbol], side, subBotTag, okSettings));

            dict[key].SetTarget(volume, price);
        }

        private string GetKey(string symbol, OrderSide side, string subBotTag)
        {
            return string.Join(symbol, "$", side.ToString() ,"%", subBotTag);
        }

    }
}
