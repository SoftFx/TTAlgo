using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [TradeBot(DisplayName = "Open Market")]
    public class OpenMarketBot : TradeBot
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        protected override void Init()
        {
        }

        protected override void OnStart()
        {
            OpenMarketOrder(Symbol.Name, OrderSide.Buy, Volume, null, null, "OpenTimeComment " + DateTime.Now);
            Exit();
        }
    }

    [TradeBot(DisplayName = "Open Limit")]
    public class OpenLimitBot : TradeBot
    {
        [Parameter(DefaultValue = 1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 100)]
        public double PriceDelta { get; set; }

        [Parameter]
        public OrderSide Side { get; set; }

        protected override void Init()
        {
            Symbol.Subscribe();
        }

        protected override void OnQuote(Quote quote)
        {
            double ordPrice = Side == OrderSide.Buy ? quote.Ask : quote.Bid;
            if (Side == OrderSide.Buy)
                ordPrice -= Symbol.Point * PriceDelta;
            else
                ordPrice += Symbol.Point * PriceDelta;
            OpenOrder(Symbol.Name, OrderType.Limit, Side, Volume, ordPrice, null, null, "OpenTimeComment " + DateTime.Now);
            Exit();
        }
    }
}
