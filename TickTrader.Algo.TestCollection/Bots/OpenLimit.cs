using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Limit Script")]
    public class OpenLimit : TradeBot
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

        protected override void Init()
        {
            double ordPrice = Side == OrderSide.Buy ? Symbol.Ask : Symbol.Bid;
            if (Side == OrderSide.Buy)
                ordPrice -= Symbol.Point * PriceDelta;
            else
                ordPrice += Symbol.Point * PriceDelta;
            OpenOrder(Symbol.Name, OrderType.Limit, Side, Volume, ordPrice, null, null, "OpenTimeComment " + DateTime.Now, Option, Tag);
            Exit();
        }
    }

    [TradeBot(DisplayName = "[T] Hit Limit By IoC")]
    public class HitLimit : TradeBot
    {
        [Parameter(DefaultValue = 1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 1)]
        public double PriceDelta { get; set; }

        protected override void Init()
        {
            double ordPrice = Symbol.Bid + Symbol.Point * PriceDelta;
            OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Buy, Volume, ordPrice, null, null, "OpenTimeComment " + DateTime.Now);
            OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Sell, Volume, ordPrice, null, null, "OpenTimeComment " + DateTime.Now, OrderExecOptions.ImmediateOrCancel);
            Exit();
        }
    }
}
