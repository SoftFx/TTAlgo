using System;
using System.IO;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection
{
    [TradeBot(DisplayName = "[P] Tester Speed Counter", Version = "1.0", Category = "Backtester")]
    public class TesterSpeedCounter : TradeBot
    {
        private ulong _totalQuotesCount;
        private DateTime _startTime;

        protected override void OnStart()
        {
            _startTime = UtcNow;
            _totalQuotesCount = 0;

            Print($"Folder - {Enviroment.BotDataFolder}");
        }

        protected override void OnQuote(Quote quote)
        {
            _totalQuotesCount++;
        }

        protected override void OnStop()
        {
            var workingTime = UtcNow - _startTime;
            var fileName = Path.Combine(Enviroment.BotDataFolder, "AlgoTester.csv");
            var speed = _totalQuotesCount / workingTime.TotalMilliseconds;

            bool fileExist = System.IO.File.Exists(fileName);

            using (var fs = new FileStream(fileName, FileMode.Append))
            {
                using (var sw = new StreamWriter(fs))
                {
                    if (!fileExist)
                        sw.WriteLine($"Symbol;Time(ms);Quotes;Speed");

                    sw.WriteLine($"{Symbol.Name};{workingTime.TotalMilliseconds:F4};{_totalQuotesCount};{speed:F10}");
                }
            }
        }
    }
}
