using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject.Bots
{
    [TradeBot(DisplayName = "Market Order Per Tick")]
    public class OnTickBot : TradeBot
    {
        [Parameter(DefaultValue = 0.1D)]
        public double Volume { get; set; }

        [Parameter]
        public OrderSide Side { get; set; }

        protected override void Init()
        {
            this.Symbol.Subscribe();
        }

        protected override void OnQuote(Quote quote)
        {
            OpenMarketOrder(Side, Volume);
        }
    }
}
