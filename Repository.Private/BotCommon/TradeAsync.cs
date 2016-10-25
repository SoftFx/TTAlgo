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

        public TradeAsync(TradeBot bot)
        {
            this.bot = bot;
        }

        public void SetLimitOrderAsync(string symbol, OrderSide side, double volume, double price, string comment, string subBotTag)
        {
            string key = GetKey(symbol, side, subBotTag);
            if (!dict.ContainsKey(key))
                dict.Add(key, new OrderKeeper(bot, bot.Symbols[symbol], side, subBotTag));

            dict[key].SetTarget(volume, price);
        }

        private string GetKey(string symbol, OrderSide side, string subBotTag)
        {
            return string.Join(symbol, "$", side.ToString() ,"%", subBotTag);
        }

    }
}
