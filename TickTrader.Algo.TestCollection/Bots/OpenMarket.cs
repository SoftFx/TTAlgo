using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "Open Market Script")]
    public class OpenMarket : TradeBot
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter]
        public OrderSide Side { get; set; }

        protected override void Init()
        {
        }

        protected override void OnStart()
        {
            OpenMarketOrder(Symbol.Name, Side, Volume, null, null, "Open Market Bot " + DateTime.Now);
            Exit();
        }
    }
}
