using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Log Test Bot", Version = "1.0", Category = "Test Bot Routine",
        SetupMainSymbol = false, Description = "Call all available log/status methods to test them.")]
    public class LogBot : TradeBot
    {
        protected override void OnStart()
        {
            Status.Write("Status.Write");
            Status.Write(" Status.Write {0} {1}", 1, 2);
            Status.WriteLine(" Status.WriteLine");
            Status.WriteLine(" Status.WriteLine {0} {1}", 1, 2);
            Print("Print");
            Print("Print {0} {1}", "1", 2);
            PrintError("PrintError");
            PrintError("PrintError {0} {1}", "1", 2);

            Exit();
        }
    }
}
