using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Calculate Order Margin Bot", Version = "1.0.0", Category = "Test Plugin Info")]
    public class CalculateOrderMarginBot : TradeBot
    {
        [Parameter(DisplayName = "OrderType", DefaultValue = OrderType.Market)]
        public OrderType OrderTypeProp { get; set; }

        [Parameter(DisplayName = "Volume", DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter(DisplayName = "OrderSide", DefaultValue = OrderSide.Buy)]
        public OrderSide OrderSideProp { get; set; }

        protected override void Init()
        {
        }

        protected override void OnQuote(Quote quote)
        {
            Print($"Order Margin: {Account.CalculateOrderMargin(Symbol.Name, OrderTypeProp, OrderSideProp, Volume, null, null, null)}");
        }
    }
}
