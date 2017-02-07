using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject.Bots
{
    [TradeBot(DisplayName = "Lucky Bot")]
    public class LuckyBot : TradeBot
    {
        [Parameter(DefaultValue = 0.1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 10)]
        public double Tp { get; set; }

        [Parameter(DefaultValue = 20)]
        public double Sl { get; set; }

        [Parameter(DefaultValue = 20)]
        public double TrailingPrice { get; set; }

        private string token;

        protected override void Init()
        {
            token = GetType().Name + ":" + Symbol.Name;

            Symbol.Subscribe();

            if (Account.Type != AccountTypes.Gross)
                Exit();
        }

        protected override void OnQuote(Quote quote)
        {
            Status.WriteLine("{0}/{1} {2}", quote.Bid, quote.Ask, quote.Time);

            var limit = Account.Orders.FirstOrDefault(o => o.Type == OrderType.Limit && o.Comment == token);
            var position = Account.Orders.FirstOrDefault(o => o.Type == OrderType.Position && o.Comment == token);

            double openPrice = quote.Ask - TrailingPrice * Symbol.Point;
            double tp = openPrice + Tp * Symbol.Point;
            double sl = openPrice - Sl * Symbol.Point;

            if (limit == null && position == null)
                OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Buy, Volume, openPrice, sl, tp, token);
            else if (limit != null)
                ModifyOrder(limit.Id, openPrice, sl, tp, token);
        }
    }
}
