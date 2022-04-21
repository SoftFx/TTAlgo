using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(Category = "Test Bot Routine", DisplayName = "[T] Test Bot Outputs", Version = "1.1",
        Description = "Draws best bids/asks on each quote")]
    public class TestBotOutputs : TradeBot
    {
        [Parameter(DefaultValue = -1)]
        public int SetBuffLength { get; set; }

        [Parameter(DefaultValue = 50)]
        public int Spread { get; set; }

        [Output(DisplayName = "Best Bid", Target = OutputTargets.Overlay, DefaultColor = Colors.Blue)]
        public DataSeries BestBid { get; set; }

        [Output(DisplayName = "Best Ask", Target = OutputTargets.Overlay, DefaultColor = Colors.Red)]
        public DataSeries BestAsk { get; set; }

        protected override void Init()
        {
            if (SetBuffLength > 0)
                SetInputSize(SetBuffLength);
        }

        protected override void OnStart()
        {
            for (int i = 0; i < Bars.Count; i++)
            {
                BestBid[i] = Bars[i].Close;
                BestAsk[i] = Bars[i].Close + Symbol.Point * Spread;
            }
        }

        protected override void OnQuote(Quote quote)
        {
            BestBid[0] = quote.Bid;
            BestAsk[0] = quote.Bid + Symbol.Point * Spread;
            Status.WriteLine($"Best Bid - {BestBid[0]}");
            Status.WriteLine($"Best Ask - {BestAsk[0]}");
            Status.Flush();
        }
    }
}
