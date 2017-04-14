using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Delay Bot", Version = "1.0", Category = "Test Plugin Info",
        Description = "Subscribes to a number of additional symbols(default 0, current chart symbol is subscribed automatically). " +
                      "Then waits for a quote and delays execution for configured number of milliseconds")]
    public class DelayBot : TradeBot
    {
        private Dictionary<string, Quote> lastQuotes = new Dictionary<string, Quote>();

        [Parameter(DisplayName = "Delay (ms)", DefaultValue = 1000)]
        public int Delay { get; set; }

        [Parameter(DisplayName = "Additional subscriptions", DefaultValue = 0)]
        public int Count { get; set; }

        protected override void OnQuote(Quote quote)
        {
            lastQuotes[quote.Symbol] = quote;
            IntercativePause();
        }

        protected override void OnStart()
        {
            int i = 0;
            foreach (var smb in Symbols)
            {
                if (smb.Name == Symbol.Name)
                    continue;

                if (i++ >= Count)
                    break;

                smb.Subscribe();
                Print("Subscribed for " + smb.Name);
            }
        }

        private void IntercativePause()
        {
            const int pause = 20;
            string quoteStr = PrintQuotes();
            int iterations = Delay / pause;
            for (int i = 0; i < iterations; i++)
            {
                Task.Delay(pause).Wait();
                PrintStatus("Waiting " + i * pause + " ms", quoteStr);
            }
            PrintStatus("Waiting quote", quoteStr);
        }

        private void PrintStatus(string pauseStatus, string symbolStatus)
        {
            Status.WriteLine(pauseStatus);
            Status.WriteLine(symbolStatus);
            Status.Flush();
        }

        private string PrintQuotes()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var quote in lastQuotes.Values)
                builder.AppendFormat("{0} {1}/{2} {3}", quote.Symbol, quote.Bid, quote.Ask, quote.Time).AppendLine();
            return builder.ToString();
        }
    }
}
