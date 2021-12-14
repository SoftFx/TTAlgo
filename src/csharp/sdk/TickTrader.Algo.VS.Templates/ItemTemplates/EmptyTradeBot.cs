using TickTrader.Algo.Api;

namespace $rootnamespace$
{
    [TradeBot(Category = "My bots", DisplayName = "$itemname$", Version = "1.0",
        Description = "My awesome $itemname$")]
    public class $safeitemname$ : TradeBot
    {
        protected override void Init()
        {
            // TO DO: Put your initialization logic here...
        }

        protected override void OnQuote(Quote quote)
        {
            // TO DO:
        }
    }
}
