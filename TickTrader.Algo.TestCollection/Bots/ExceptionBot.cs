using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Exception Bot")]
    public class ExceptionBot : TradeBot
    {
        protected override void Init()
        {
            this.Account.BalanceUpdated += () => { throw new Exception("Test exception!"); };
            this.Account.Orders.Filled += a => { throw new Exception("Test exception!"); };
            this.Account.Orders.Opened += a => { throw new Exception("Test exception!"); };

            throw new Exception("Test exception!");
        }

        protected override void OnStart()
        {
            throw new Exception("Test exception!");
        }

        protected override void OnStop()
        {
            throw new Exception("Test exception!");
        }

        protected override void OnQuote(Quote quote)
        {
            throw new Exception("Test exception!");
        }
    }
}
