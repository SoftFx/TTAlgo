using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Subscription Test Bot", Version = "1.1", Category = "Test Plugin Info",
        Description = "Subcribes to specified number of symbols with specified depth. " +
                      "Prints number of quotes that already came, lastest quote time and depth to bot status window")]
    public class SubscriptionTestBot : TradeBot
    {
        private Dictionary<string, QuoteStats> _snapshot;

        [Parameter(DefaultValue = 1)]
        public int Depth { get; set; }

        [Parameter(DefaultValue = 10)]
        public int Count { get; set; }

        protected override void Init()
        {
            Symbol.Unsubscribe();

            _snapshot = Symbols
                .Take(Count)
                .Select(s => new QuoteStats(s))
                .ToDictionary(s => s.Symbol);

            foreach (var stats in _snapshot.Values)
                Feed.Subscribe(stats.Symbol, Depth);

            PrintSnapshot();
        }

        protected override void OnQuote(Quote quote)
        {
            if (_snapshot.TryGetValue(quote.Symbol, out var stats))
            {
                stats.Count++;
                PrintSnapshot();
            }
        }

        private void PrintSnapshot()
        {
            foreach (var stats in _snapshot.Values)
                stats.Print(Status);
        }

        private class QuoteStats
        {
            public QuoteStats(Symbol descriptor)
            {
                Info = descriptor;
            }

            public string Symbol => Info.Name;
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
