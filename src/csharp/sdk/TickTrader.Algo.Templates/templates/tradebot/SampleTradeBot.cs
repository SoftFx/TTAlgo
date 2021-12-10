using TickTrader.Algo.Api;

namespace SampleTradeBot
{
    [TradeBot(Category = "My bots", DisplayName = "SampleTradeBot", Version="1.0",
        Description = "My own SampleTradeBot")]
    public class SampleTradeBot : TradeBot
    {
        protected override void OnStart()
        {
            Status.WriteLine("Hello world!");
        }
    }
}
