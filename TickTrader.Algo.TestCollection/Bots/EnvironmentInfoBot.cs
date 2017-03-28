using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Environment Info Bot")]
    public class EnvironmentInfoBot : TradeBotCommon
    {
        protected override void Init()
        {
            Status.WriteLine(ToObjectPropertiesString("Environment Info", typeof(EnvironmentInfo), Enviroment));
        }
    }
}