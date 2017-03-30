using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Order Script")]
    public class OpenOrder : TradeBotCommon
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 0.0)]
        public double Price { get; set; }

        [Parameter(DefaultValue = OrderSide.Buy)]
        public OrderSide Side { get; set; }

        [Parameter(DefaultValue = OrderType.Market)]
        public OrderType Type { get; set; }

        [Parameter(DefaultValue = OrderExecOptions.None)]
        public OrderExecOptions Options { get; set; }

        [Parameter]
        public string Tag { get; set; }


        protected override void OnStart()
        {
            var price = Math.Abs(Price) > Symbol.Point ? Price : (Side == OrderSide.Buy ? Symbol.Ask : Symbol.Bid);
            var res = OpenOrder(Symbol.Name, Type, Side, Volume, price, null, null, "Open Order Bot " + DateTime.Now, Options, Tag);
            Status.WriteLine($"ResultCode = {res.ResultCode}");
            if (res.ResultingOrder != null)
            {
                Status.WriteLine(ToObjectPropertiesString(res.ResultingOrder));
            }
            Exit();
        }
    }
}