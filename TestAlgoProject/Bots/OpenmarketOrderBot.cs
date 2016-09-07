using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [TradeBot(DisplayName = "Open Market")]
    public class OpenOrderBot : TradeBot
    {
        [Parameter(DefaultValue = 1000D)]
        public double Volume { get; set; }

        protected override void Init()
        {
        }

        protected override void OnStart()
        {
            OpenMarketOrder(Symbol.Name, OrderSide.Buy, 1);
            this.
            Exit();
        }
    }
}
