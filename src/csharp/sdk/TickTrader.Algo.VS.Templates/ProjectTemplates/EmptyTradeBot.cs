using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace $safeprojectname$
{
    [TradeBot(Category = "My bots", DisplayName = "$projectname$", Version = "1.0",
        Description = "My awesome $projectname$")]
    public class $safeprojectname$ : TradeBot
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
