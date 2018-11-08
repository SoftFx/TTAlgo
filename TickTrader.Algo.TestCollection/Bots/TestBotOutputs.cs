using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(Category = "Test Bot Routine", DisplayName = "[T] Test Bot Outputs", Version = "1.0",
        Description = "Draws best bids/asks on each quote")]
    public class TestBotOutputs : TradeBot
    {
        [Output(DisplayName = "Best Bid", Target = OutputTargets.Overlay, DefaultColor = Colors.Blue)]
        public DataSeries BestBid { get; set; }

        [Output(DisplayName = "Best Ask", Target = OutputTargets.Overlay, DefaultColor = Colors.Red)]
        public DataSeries BestAsk { get; set; }


        protected override void OnStart()
        {
            for (var i = Bars.Count - 1; i >= 0; i--)
            {
                BestBid[i] = Symbol.Bid;
                BestAsk[i] = Symbol.Ask;
            }
        }

        protected override void OnQuote(Quote quote)
        {
            BestBid[0] = quote.Bid;
            BestAsk[0] = quote.Ask;
            Status.WriteLine($"Best Ask - {BestAsk.Count}");
            Status.WriteLine($"Best Bid - {BestBid.Count}");
            Status.Flush();
        }
    }
}
