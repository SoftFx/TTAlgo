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
        public double TpDelta { get; set; }

        [Parameter(DefaultValue = 20)]
        public double SlDelta { get; set; }

        [Parameter(DefaultValue = 20)]
        public double PriceDelta { get; set; }

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
            var limit = Account.Orders.FirstOrDefault(o => o.Type == OrderType.Limit && o.Comment == token);
            var position = Account.Orders.FirstOrDefault(o => o.Type == OrderType.Position && o.Comment == token);

            double openPrice = quote.Bid - PriceDelta * Symbol.Point;
            double tp = openPrice + TpDelta * Symbol.Point;
            double sl = openPrice - SlDelta * Symbol.Point;

            if (limit == null && position == null)
                OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Buy, Volume, openPrice, sl, tp, token);
            else if (limit != null)
            {
                var result = ModifyOrder(limit.Id, openPrice, sl, tp, token);
                if (result.IsCompleted)
                    Print("Order #" + limit.Id + " modified price =" + openPrice + " sl=" + sl + "tp=" + tp);
                else
                    Print("Failed to modify order #" + limit.Id + " err=" + result.ResultCode);
            }
        }
    }
}
