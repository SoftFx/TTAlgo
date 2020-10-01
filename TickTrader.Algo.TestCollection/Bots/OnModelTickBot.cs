using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] On Model Tick Bot", Version = "1.0", Category = "Test Bot Routine",
        Description = "Prints when OnModelTick is called")]
    public class OnModelTickBot : TradeBot
    {
        protected override void OnModelTick()
        {
            var sb = new StringBuilder();
            sb.Append($"OnModelTick: {Now:yyyy-MM-dd HH:mm:ss.fff}");
            Print(sb.ToString());
            sb.AppendLine();
            var bar = Bars[0];
            sb.AppendLine("Last bar:");
            sb.AppendLine($"Open time - {bar.OpenTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Close time - {bar.CloseTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Open - {bar.Open}");
            sb.AppendLine($"High - {bar.High}");
            sb.AppendLine($"Low - {bar.Low}");
            sb.AppendLine($"Close - {bar.Close}");
            Status.WriteLine(sb.ToString());
        }
    }
}
