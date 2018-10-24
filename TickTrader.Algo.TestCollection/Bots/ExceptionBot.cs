using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    public enum ThrowVariants { OnInit, OnStart, OnQuote, OnStop, OnAsyncStop, OnTradeEvents }

    [TradeBot(DisplayName = "[T] Exception Bot", Version = "1.1", Category = "Test Bot Routine", SetupMainSymbol = false,
        Description = "Throw exception on init, start, stop, new quote, balance update, order filled and order opened")]
    public class ExceptionBot : TradeBot
    {
        [Parameter(DisplayName = "Throw on")]
        public ThrowVariants ThrowOn { get; set; }

        protected override void Init()
        {
            Account.BalanceUpdated += () => Throw(ThrowVariants.OnTradeEvents);
            Account.Orders.Filled += a => Throw(ThrowVariants.OnTradeEvents);
            Account.Orders.Opened += a => Throw(ThrowVariants.OnTradeEvents);
            Account.Orders.Canceled += a => Throw(ThrowVariants.OnTradeEvents);
            Account.Orders.Activated += a => Throw(ThrowVariants.OnTradeEvents);
            Account.Orders.Expired += a => Throw(ThrowVariants.OnTradeEvents);
            Account.Orders.Closed += a => Throw(ThrowVariants.OnTradeEvents);
            Account.Orders.Modified += a => Throw(ThrowVariants.OnTradeEvents);
            Account.Orders.Replaced += a => Throw(ThrowVariants.OnTradeEvents);

            Account.Orders.Added += a => Throw(ThrowVariants.OnTradeEvents);
            Account.Orders.Removed += a => Throw(ThrowVariants.OnTradeEvents);

            Throw(ThrowVariants.OnInit);
        }

        protected override void OnStart()
        {
            Throw(ThrowVariants.OnStart);
        }

        protected override async Task AsyncStop()
        {
            await Task.Delay(100);
            Throw(ThrowVariants.OnAsyncStop);
        }

        protected override void OnStop()
        {
            Throw(ThrowVariants.OnStop);
        }

        protected override void OnQuote(Quote quote)
        {
            Throw(ThrowVariants.OnQuote);
        }

        private void Throw(ThrowVariants place)
        {
            if (place == ThrowOn)
                throw new Exception("Test exception!");
        }
    }
}
