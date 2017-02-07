using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Subscription Test Bot")]
    public class SubscriptionTestBot : TradeBot
    {
        private Dictionary<string, QuoteStats> snapshot;

        [Parameter(DefaultValue = 1)]
        public int Depth { get; set; }

        [Parameter(DefaultValue = 10)]
        public int Count { get; set; }

        protected override void Init()
        {
            Symbol.Unsubscribe();

            snapshot = Symbols
                .Take(Count)
                .Select(s => new QuoteStats(s))
                .ToDictionary(s => s.Symbol);

            foreach (var stats in snapshot.Values)
                Feed.Subscribe(stats.Symbol, Depth);

            PrintSnapshot();
        }

        protected override void OnQuote(Quote quote)
        {
            QuoteStats stats;
            if (snapshot.TryGetValue(quote.Symbol, out stats))
            {
                stats.Count++;
                PrintSnapshot();
            }
        }

        private void PrintSnapshot()
        {
            foreach (var stats in snapshot.Values)
                stats.Print(Status);
        }

        private class QuoteStats
        {
            public QuoteStats(Symbol descriptor)
            {
                this.Info = descriptor;
            }

            public string Symbol { get { return Info.Name; } }
            public int Count { get; set; }
            public Symbol Info { get; set; }

            public void Print(StatusApi status)
            {
                if (double.IsNaN(Info.Ask) && double.IsNaN(Info.Bid))
                    status.WriteLine("{0} - off quote", Symbol);
                else
                {
                    var lastQuote = Info.LastQuote;
                    var depth = Math.Max(lastQuote.BidBook.Length, lastQuote.AskBook.Length);
                    status.WriteLine("{0} {1} depth={2} count={3}", Symbol, lastQuote.Time, depth, Count);
                }
            }
        }
    }
}
