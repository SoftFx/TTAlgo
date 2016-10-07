using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject.Bots
{
    [TradeBot(DisplayName = "Subscription Test Bot")]
    public class SubscriptionTestBot : TradeBot
    {
        private Dictionary<string, QuoteStats> snapshot = new Dictionary<string, QuoteStats>();

        [Parameter(DefaultValue = 1)]
        public int Depth { get; set; }

        [Parameter(DefaultValue = 10)]
        public int Count { get; set; }

        protected override void Init()
        {
            Symbol.Unsubscribe();

            int i = 0;

            foreach (var symbol in Symbols)
            {
                if (i >= Count)
                    break;

                symbol.Subscribe(Depth);
                snapshot.Add(symbol.Name, new QuoteStats(symbol));
                Print("Subscribed for " + symbol.Name);

                i++;
            }

            PrintSnapshot();
        }

        protected override void OnQuote(Quote quote)
        {
            base.OnQuote(quote);

            QuoteStats targetStats;
            if (snapshot.TryGetValue(quote.Symbol, out targetStats))
            {
                targetStats.Count++;
                targetStats.LastDepth = Math.Max(quote.BidBook.Length, quote.AskBook.Length);
                targetStats.LastTime = quote.Time;
                PrintSnapshot();
            }
        }

        private void PrintSnapshot()
        {
            foreach (var stats in snapshot.Values)
                Status.WriteLine("{0} c{3} d{2} {1}", stats.Symbol, (object)stats.LastTime ?? "", stats.LastDepth, stats.Count);
        }

        private class QuoteStats
        {
            public QuoteStats(Symbol descriptor)
            {
                this.Descriptor = descriptor;
            }

            public string Symbol { get { return Descriptor.Name; } }
            public int Count { get; set; }
            public int LastDepth { get; set; }
            public DateTime? LastTime { get; set; }
            public Symbol Descriptor { get; set; }
        }
    }
}
