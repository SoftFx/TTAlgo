using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Environment Info Bot", Version = "1.1", Category = "Test Plugin Info",
        SetupMainSymbol = false, Description = "Prints environment info to bot status window")]
    public class EnvironmentInfoBot : TradeBotCommon
    {
        protected override void Init()
        {
            var sbuilder = new StringBuilder();
            sbuilder.AppendLine($"Bot Instance ID: {Id}");
            sbuilder.AppendLine(ToObjectPropertiesString("Environment Info", typeof(EnvironmentInfo), Enviroment));
            Status.WriteLine(sbuilder.ToString());
        }
    }
}