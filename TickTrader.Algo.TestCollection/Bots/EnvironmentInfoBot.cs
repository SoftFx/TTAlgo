using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Environment Info Bot", Version = "1.0", Category = "Test Plugin Info",
        Description = "Prints environment info to bot status window")]
    public class EnvironmentInfoBot : TradeBotCommon
    {
        protected override void Init()
        {
            Status.WriteLine(ToObjectPropertiesString("Environment Info", typeof(EnvironmentInfo), Enviroment));
        }
    }
}