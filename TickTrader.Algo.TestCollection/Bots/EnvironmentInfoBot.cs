using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Environment Info Bot", Version = "1.2", Category = "Test Plugin Info",
        Description = "Prints environment info to bot status window")]
    public class EnvironmentInfoBot : TradeBotCommon
    {
        protected override void OnStart()
        {
            var sbuilder = new StringBuilder();
            sbuilder.AppendLine($"Bot Instance ID: {Id}");
            sbuilder.AppendLine($"Main Symbol: {Symbol.Name}");
            sbuilder.AppendLine($"TimeFrame: {TimeFrame}");
            sbuilder.AppendLine();
            sbuilder.AppendLine(ToObjectPropertiesString("Environment Info", Enviroment));
            sbuilder.AppendLine(ToObjectPropertiesString("Diagnostics Info", Diagnostics));
            Status.WriteLine(sbuilder.ToString());
            Exit();
        }
    }
}