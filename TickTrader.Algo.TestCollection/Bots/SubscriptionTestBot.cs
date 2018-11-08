using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    public enum SubscribeMethod { FeedProvider = 0, SymbolAccessor = 1 };


    [TradeBot(DisplayName = "[T] Subscription Test Bot", Version = "1.2", Category = "Test Plugin Info", SetupMainSymbol = false,
        Description = "Subcribes to specified number of symbols with specified depth. " +
                      "Prints number of quotes that already came, lastest quote time and depth to bot status window")]
    public class SubscriptionTestBot : TradeBot
    {
        private Dictionary<string, QuoteStats> _snapshot;
        private CancellationTokenSource _cancelTokenSrc;


        [Parameter(DisplayName = "Min Depth", DefaultValue = -1)]
        public int MinDepth { get; set; }

        [Parameter(DisplayName = "Max Depth", DefaultValue = 3)]
        public int MaxDepth { get; set; }

        [Parameter(DisplayName = "Subcribe Timeout (ms)", DefaultValue = 10000)]
        public int SubscribeTimeout { get; set; }

        [Parameter(DisplayName = "Symbol Count", DefaultValue = 10)]
        public int Count { get; set; }

        [Parameter(DisplayName = "Subcribe Method", DefaultValue = SubscribeMethod.FeedProvider)]
        public SubscribeMethod SubscribeMethod { get; set; }


        protected override void Init()
        {
            Symbol.Unsubscribe();

            _snapshot = Symbols
                .Take(Count)
                .Select(s => new QuoteStats(s))
                .ToDictionary(s => s.Symbol);

            _cancelTokenSrc = new CancellationTokenSource();

            SnapshotLoop();
            SubscribeLoop();
        }

        protected override void OnStop()
        {
            try
            {
                _cancelTokenSrc.Cancel();
            }
            catch (Exception ex)
            {
                PrintError($"Failed to stop bot gracefully: {ex.Message}");
            }
        }

        protected override void OnQuote(Quote quote)
        {
            if (_snapshot == null)
            {
                return;
            }

            if (_snapshot.TryGetValue(quote.Symbol, out var stats))
            {
                stats.Count++;
            }
        }


        private async void SnapshotLoop()
        {
            while (!IsStopped)
            {
                PrintSnapshot();
                await Task.Delay(100);
            }
        }

        private void PrintSnapshot()
        {
            foreach (var stats in _snapshot.Values)
                stats.Print(Status);
        }

        private async void SubscribeLoop()
        {
            if (SubscribeTimeout <= 0)
            {
                SubcribeAll(MaxDepth);
                return;
            }

            try
            {
                while (!IsStopped)
                {
                    if (MinDepth == MaxDepth)
                    {
                        SubcribeAll(MaxDepth);
                        await Task.Delay(SubscribeTimeout, _cancelTokenSrc.Token);
                    }
                    else
                    {
                        for (var i = MaxDepth; i > MinDepth; i--)
                        {
                            SubcribeAll(i);

                            await Task.Delay(SubscribeTimeout, _cancelTokenSrc.Token);
                        }
                        for (var i = MinDepth; i < MaxDepth; i++)
                        {
                            SubcribeAll(i);

                            await Task.Delay(SubscribeTimeout, _cancelTokenSrc.Token);
                        }
                    }
                }
            }
            catch (TaskCanceledException) { }
        }

        private void SubcribeAll(int depth)
        {
            try
            {
                switch (SubscribeMethod)
                {
                    case SubscribeMethod.FeedProvider:
                        FeedSubcribeAll(depth);
                        break;
                    case SubscribeMethod.SymbolAccessor:
                        SymbolSubcribeAll(depth);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                PrintError($"Error on changing subcription: {ex.Message}");
            }
        }

        private void SymbolSubcribeAll(int depth)
        {
            foreach (var stats in _snapshot.Values)
            {
                if (depth > -1)
                    stats.Info.Subscribe(depth);
                else
                    stats.Info.Unsubscribe();
                stats.CurrentDepth = depth;
            }
        }

        private void FeedSubcribeAll(int depth)
        {
            foreach (var stats in _snapshot.Values)
            {
                if (depth > -1)
                    Feed.Subscribe(stats.Symbol, depth);
                else
                    Feed.Unsubscribe(stats.Symbol);
                stats.CurrentDepth = depth;
            }
        }


        private class QuoteStats
        {
            public string Symbol => Info.Name;

            public int Count { get; set; }

            public Symbol Info { get; set; }

            public int CurrentDepth { get; set; }


            public QuoteStats(Symbol descriptor)
            {
                Info = descriptor;
            }

            public void Print(StatusApi status)
            {
                if (double.IsNaN(Info.Ask) && double.IsNaN(Info.Bid))
                    status.WriteLine("{0} - off quote", Symbol);
                else
                {
                    var lastQuote = Info.LastQuote;
                    var depth = Math.Max(lastQuote.BidBook.Length, lastQuote.AskBook.Length);
                    status.WriteLine($"{Symbol}: depth={CurrentDepth}({lastQuote.BidBook.Length}/{lastQuote.AskBook.Length}), count={Count}, {lastQuote.Time}");
                }
            }
        }
    }
}
