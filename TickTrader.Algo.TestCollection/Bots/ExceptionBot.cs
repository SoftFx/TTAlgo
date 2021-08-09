using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    public enum ThrowVariants { OnInit, OnStart, OnQuote, OnStop, OnAsyncStop, OnTradeEvents, OnOrderCollectionsUpdate }

    [TradeBot(DisplayName = "[T] Exception Bot", Version = "1.2", Category = "Test Bot Routine", SetupMainSymbol = false,
        Description = "Throw exception on init, start, stop, new quote, balance update, order filled and order opened")]
    public class ExceptionBot : TradeBot
    {
        [Parameter(DisplayName = "Throw on")]
        public ThrowVariants ThrowOn { get; set; }

        protected override void Init()
        {
            Account.BalanceUpdated += () => Throw(ThrowVariants.OnTradeEvents, "BalanceUpdated");

            Account.Assets.Modified += a => Throw(ThrowVariants.OnTradeEvents, "AssetChange");

            Account.Orders.Filled += a => Throw(ThrowVariants.OnTradeEvents, "OrderFilled");
            Account.Orders.Opened += a => Throw(ThrowVariants.OnTradeEvents, "OrderOpened");
            Account.Orders.Canceled += a => Throw(ThrowVariants.OnTradeEvents, "OrderCanceled");
            Account.Orders.Activated += a => Throw(ThrowVariants.OnTradeEvents, "OrderActivated");
            Account.Orders.Expired += a => Throw(ThrowVariants.OnTradeEvents, "OrderExpired");
            Account.Orders.Closed += a => Throw(ThrowVariants.OnTradeEvents, "OrderClosed");
            Account.Orders.Modified += a => Throw(ThrowVariants.OnTradeEvents, "OrderModified");
            Account.Orders.Replaced += a => Throw(ThrowVariants.OnTradeEvents, "OrderReplaced");

            Account.NetPositions.Splitted += a => Throw(ThrowVariants.OnTradeEvents, "PositionSplitted");
            Account.NetPositions.Modified += a => Throw(ThrowVariants.OnTradeEvents, "PositionModified");

            Account.Orders.Added += a => Throw(ThrowVariants.OnOrderCollectionsUpdate, "OrderAdded");
            Account.Orders.Replaced += a => Throw(ThrowVariants.OnOrderCollectionsUpdate, "OrderReplaced");
            Account.Orders.Removed += a => Throw(ThrowVariants.OnOrderCollectionsUpdate, "OrderRemoved");

            Throw(ThrowVariants.OnInit, nameof(Init));
        }

        protected override void OnStart()
        {
            Throw(ThrowVariants.OnStart, nameof(OnStart));
        }

        protected override async Task AsyncStop()
        {
            await Task.Delay(100);
            Throw(ThrowVariants.OnAsyncStop, nameof(AsyncStop));
        }

        protected override void OnStop()
        {
            Throw(ThrowVariants.OnStop, nameof(OnStop));
        }

        protected override void OnQuote(Quote quote)
        {
            Throw(ThrowVariants.OnQuote, nameof(OnQuote));
        }

        private void Throw(ThrowVariants place, string eventName)
        {
            if (place == ThrowOn)
                throw new Exception($"Test exception! Event {eventName}");
        }
    }
}
