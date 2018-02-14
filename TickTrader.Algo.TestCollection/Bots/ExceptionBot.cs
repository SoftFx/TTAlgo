using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Exception Bot", Version = "1.0", Category = "Test Bot Routine",
        Description = "Throw exception on init, start, stop, new quote, balance update, order filled and order opened")]
    public class ExceptionBot : TradeBot
    {
        protected override void Init()
        {
            Account.BalanceUpdated += () => { throw new Exception("Test exception!"); };
            Account.Orders.Filled += a => { throw new Exception("Test exception!"); };
            Account.Orders.Opened += a => { throw new Exception("Test exception!"); };

            throw new Exception("Test exception!");
        }

        protected override void OnStart()
        {
            throw new Exception("Test exception!");
        }

        protected override async Task AsyncStop()
        {
            await Task.Delay(100);
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
