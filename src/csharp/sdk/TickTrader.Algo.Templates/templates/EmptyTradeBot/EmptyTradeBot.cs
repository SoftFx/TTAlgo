using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace EmptyTradeBot
{
    [TradeBot(Category = "My bots", DisplayName = "EmptyTradeBot", Version = "1.0",
        Description = "My awesome EmptyTradeBot")]
    public class EmptyTradeBot : TradeBot
    {
        protected override void Init()
        {
            // TO DO: Put your initialization logic here...
        }

        protected override void OnStart()
        {
            Print("Hello world!!!");
            // TO DO: Put your logic here...
        }

        protected override Task AsyncStop()
        {
            // TO DO: Put your logic here...

            return base.AsyncStop();
        }

        protected override void OnStop()
        {
            // TO DO: Put your logic here...
        }

        protected override void OnQuote(Quote quote)
        {
            // TO DO: Put your logic here...
        }

        protected override void OnModelTick()
        {
            // TO DO: Put your logic here...
        }
    }
}
