using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject.Bots
{
    [TradeBot(DisplayName = "New Order Per Tick")]
    public class OnTickBot : TradeBot
    {
        protected override void Init()
        {
            this.Symbol.Subscribe();
        }

        protected override void OnQuote(Quote quote)
        {
            OpenMarketOrder(OrderSide.Buy, 1);
        }
    }
}
