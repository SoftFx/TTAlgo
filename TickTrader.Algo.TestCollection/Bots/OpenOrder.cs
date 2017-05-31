using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Order Script", Version = "2.3", Category = "Test Orders",
        Description = "Opens order for current chart symbol with specified volume, price, side, type, options, tag, SL, TP. " +
                      "Prints order execution result to bot status window. " +
                      "If price = 0 then it will be taken from symbol bid/ask (depending on order side). " +
                      "If SL or TP = 0 then they won't be sent in order request")]
    public class OpenOrder : TradeBotCommon
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
            var price = Price.Gt(Symbol.Point) ? Price : GetCurrentPrice(Side);
            var sl = StopLoss.Gt(Symbol.Point) ? StopLoss : (double?)null;
            var tp = TakeProfit.Gt(Symbol.Point) ? TakeProfit : (double?)null;
            if (double.IsNaN(price))
                PrintError("Cannot open order: off quotes for " + Symbol.Name);
            else
            {
                var res = OpenOrder(Symbol.Name, Type, Side, Volume, price, sl, tp, "Open Order Bot " + DateTime.Now, Options, Tag);
                Status.WriteLine($"ResultCode = {res.ResultCode}");
                if (res.ResultingOrder != null)
                    Status.WriteLine(ToObjectPropertiesString(res.ResultingOrder));
            }
            Exit();
        }
    }
}