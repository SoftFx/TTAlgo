using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Market Script", Version = "1.1", Category = "Test Orders",
        Description = "Opens market order for current chart symbol with specified volume, side and tag")]
    public class OpenMarket : TradeBot
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter]
        public OrderSide Side { get; set; }

        [Parameter]
        public string Tag { get; set; }

        [Parameter(DefaultValue = false)]
        public bool DoNotExit { get; set; }

        protected override void Init()
        {
            var price = Side == OrderSide.Buy ? Symbol.Ask : Symbol.Bid;
            if (double.IsNaN(price))
                price = 1; // can still try

            OpenOrder(Symbol.Name, OrderType.Market, Side, Volume, price, null, null, "Open Market Bot " + DateTime.Now, OrderExecOptions.None, Tag);

            if (!DoNotExit)
            {
                Exit();
                return;
            }
        }

        protected override void OnStart()
        {

        }

        protected override double GetOptimizationMetric()
        {
            return Account.Equity / 4;
        }
    }
}
