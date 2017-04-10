using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Order Script")]
    public class OpenOrder : TradeBot
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 0.0)]
        public double Price { get; set; }

        [Parameter(DefaultValue = OrderSide.Buy)]
        public OrderSide Side { get; set; }

        [Parameter(DefaultValue = OrderType.Limit)]
        public OrderType Type { get; set; }

        [Parameter(DefaultValue = OrderExecOptions.None)]
        public OrderExecOptions Options { get; set; }

        [Parameter(DefaultValue = "OpenOrderBot0")]
        public string Tag { get; set; }

        [Parameter(DisplayName = "Stop Loss", DefaultValue = 0.0, IsRequired = false)]
        public double StopLoss { get; set; }

        [Parameter(DisplayName = "Take Profit", DefaultValue = 0.0, IsRequired = false)]
        public double TakeProfit { get; set; }


        protected override void OnStart()
        {
            var price = Price.Gt(Symbol.Point) ? Price : (Side == OrderSide.Buy ? Symbol.Ask : Symbol.Bid);
            var sl = StopLoss.Gt(Symbol.Point) ? StopLoss : (double?)null;
            var tp = TakeProfit.Gt(Symbol.Point) ? TakeProfit : (double?)null;
            OpenOrder(Symbol.Name, Type, Side, Volume, price, sl, tp, "Open Order Bot " + DateTime.Now, Options, Tag);
            Exit();
        }
    }
}