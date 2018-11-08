using System.Diagnostics;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Bot Log Writer", Version = "1.0", Category = "Plugin Stress Tests",
        SetupMainSymbol = false, Description = "Writes messages to log with specified rate per second.")]

    public class BotLogWriter : TradeBot
    {
        [Parameter(DefaultValue = 100)]
        public int PrintsPerSecond { get; set; }

        protected async override void OnStart()
        {
            var watch = new Stopwatch();

            long seed = 0;

            while (!IsStopped)
            {
                watch.Restart();
                for (int i = 0; i < PrintsPerSecond; i++)
                {
                    var no = seed++;
                    Print("Journal Stress Test Record" + no);
                }
                watch.Stop();

                var toWait = 1000 - watch.ElapsedMilliseconds;
                if (toWait < 1)
                {
                    toWait = 1;
                    PrintError("Actual message per second: " + 1000 * PrintsPerSecond / watch.ElapsedMilliseconds);
                }

                await Delay((int)toWait);
            }
        }
    }
}
