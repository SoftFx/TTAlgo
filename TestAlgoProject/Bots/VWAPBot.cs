using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject.Bots
{
    [TradeBot(DisplayName = "VWAP Bot")]
    public class VWAPBot : TradeBot
    {
        [Parameter(DefaultValue = 1000.0)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 5)]
        public int Depth { get; set; }

        protected override void Init()
        {
            Symbol.Subscribe(Depth);
        }

        protected override void OnQuote(Quote quote)
        {
            Status.WriteLine("{0} {1} {2}/{3}", Symbol.Name, quote.Time, quote.Bid, quote.Ask);
            PrintBookSide(quote.AskBook, "Ask", true);
            PrintBookSide(quote.BidBook, "Bid", false);
        }

        private void PrintBookSide(IReadOnlyList<BookEntry> book, string sideName, bool reverse)
        {
            var sortedBook = reverse ? book.Reverse() : book;

            foreach (var entry in sortedBook)
                Status.WriteLine("{0} {1} {2}", Symbol.FormatPrice(entry.Price), sideName, entry.Volume);
        }

        private double VWAPFunc(IReadOnlyList<BookEntry> entry)
        {
            return 0;
        }
    }
}
