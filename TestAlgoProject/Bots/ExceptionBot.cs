using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject.Bots
{
    [TradeBot(DisplayName = "Exception Bot")]
    public class ExceptionBot : TradeBot
    {
        protected override void Init()
        {
            this.Account.BalanceUpdated += () => { throw new Exception("Fail!"); };
            this.Account.Orders.Filled += a => { throw new Exception("Fail!"); };
            this.Account.Orders.Opened += a => { throw new Exception("Fail!"); };

            throw new Exception("Fail!");
        }

        protected override void OnStart()
        {
            throw new Exception("Fail!");
        }

        protected override void OnStop()
        {
            throw new Exception("Fail!");
        }

        protected override void OnQuote(Quote quote)
        {
            throw new Exception("Failed!");
        }
    }
}
