using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Limit Script", Version = "1.2", Category = "Test Orders",
        Description = "Opens limit order for current chart symbol with specified volume, side and options. " +
                      "Price is moved for specified number of pips into spread")]
    public class OpenLimit : TradeBotCommon
    {
        [Parameter(DefaultValue = 1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 100)]
        public double PriceDelta { get; set; }

        [Parameter]
        public OrderSide Side { get; set; }

        [Parameter]
        public OrderExecOptions Option { get; set; }

        [Parameter]
        public string Tag { get; set; }

        protected async override void Init()
        {
            var ordPrice = await GetCurrentPrice(Side);
            if (Side == OrderSide.Buy)
                ordPrice -= Symbol.Point * PriceDelta;
            else
                ordPrice += Symbol.Point * PriceDelta;
            if (double.IsNaN(ordPrice))
                PrintError("Cannot open order: off quotes for " + Symbol.Name);
            else
                OpenOrder(Symbol.Name, OrderType.Limit, Side, Volume, ordPrice, null, null, "OpenTimeComment " + DateTime.Now, Option, Tag);
            Exit();
        }
    }

    [TradeBot(DisplayName = "[T] Hit Limit By IoC", Version = "1.0", Category = "Test Orders",
        Description = "Opens buy limit for current chart symbol with specified volume and then tries to take it by sell limit IoC. " +
                      "Price is moved for specified number of pips into spread")]
    public class HitLimit : TradeBot
    {
        [Parameter(DefaultValue = 1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 1)]
        public double PriceDelta { get; set; }

        protected override void Init()
        {
            var ordPrice = Symbol.Bid + Symbol.Point * PriceDelta;
            OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Buy, Volume, ordPrice, null, null, "OpenTimeComment " + DateTime.Now);
            OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Sell, Volume, ordPrice, null, null, "OpenTimeComment " + DateTime.Now, OrderExecOptions.ImmediateOrCancel);
            Exit();
        }
    }
}
