using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "Open Limit Script")]
    public class OpenLimit : TradeBot
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
