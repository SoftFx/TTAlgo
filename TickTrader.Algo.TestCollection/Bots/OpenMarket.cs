using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Market Script", Version = "1.0", Category = "Test Orders",
        Description = "Opens market order for current chart symbol with specified volume, side and tag")]
    public class OpenMarket : TradeBot
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter]
        public OrderSide Side { get; set; }

        [Parameter]
        public string Tag { get; set; }

        protected override void Init()
        {
        }

        protected override void OnStart()
        {
            OpenOrder(Symbol.Name, OrderType.Market, Side, Volume, Symbol.Ask, null, null, "Open Market Bot " + DateTime.Now, OrderExecOptions.None, Tag);
            Exit();
        }
    }
}
