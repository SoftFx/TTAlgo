using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject.Bots
{
    [TradeBot()]
    public class KeepLimitBot : TradeBot
    {
        [Parameter(DefaultValue = 1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 100)]
        public double PriceDelta { get; set; }

        [Parameter(DefaultValue = OrderSide.Sell)]
        public OrderSide Side { get; set; }

        protected override void OnQuote(Quote quote)
        {
            var order = Account.Orders.FirstOrDefault(o => o.Type == OrderType.Limit && o.Symbol == Symbol.Name);
            var targetPrice = GetTargetPrice(quote);
            if (double.IsNaN(targetPrice) || targetPrice == 0)
                return;
            if (order == null)
                OpenOrder(Symbol.Name, OrderType.Limit, Side, Volume, targetPrice);
            else if (order.Price != targetPrice)
                ModifyOrder(order.Id, targetPrice);
        }

        private double GetTargetPrice(Quote q)
        {
            double ordPrice = Side == OrderSide.Buy ? q.Ask : q.Bid;
            if (Side == OrderSide.Buy)
                return ordPrice - Symbol.Point * PriceDelta;
            else
                return ordPrice + Symbol.Point * PriceDelta;
        }
    }

    [TradeBot]
    public class KeepLimitAsyncBot : TradeBot
    {
        private bool isBusy;
        private Quote newQuote;

        [Parameter(DefaultValue = 1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 100)]
        public double PriceDelta { get; set; }

        [Parameter]
        public OrderSide Side { get; set; }

        protected async override void OnQuote(Quote quote)
        {
            newQuote = quote;

            if (isBusy)
                return;

            isBusy = true;

            while (newQuote != null)
            {
                var targetPrice = GetTargetPrice(newQuote);

                newQuote = null;

                if (double.IsNaN(targetPrice) || targetPrice == 0)
                    return;

                var order = Account.Orders.FirstOrDefault(o => o.Type == OrderType.Limit && o.Symbol == Symbol.Name);

                if (order == null)
                    await OpenOrderAsync(Symbol.Name, OrderType.Limit, Side, Volume, targetPrice);
                else if (order.Price != targetPrice)
                    await ModifyOrderAsync(order.Id, targetPrice);
            }

            isBusy = false;
        }

        private double GetTargetPrice(Quote q)
        {
            double ordPrice = Side == OrderSide.Buy ? q.Ask : q.Bid;
            if (Side == OrderSide.Buy)
                return ordPrice - Symbol.Point * PriceDelta;
            else
                return ordPrice + Symbol.Point * PriceDelta;
        }
    }
}
