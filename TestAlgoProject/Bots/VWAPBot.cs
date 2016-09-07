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
        private StringBuilder builder = new StringBuilder();

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
            builder.Clear();
            builder.Append(Symbol.Name).Append(" ").Append(quote.Time).Append(" ").Append(quote.Bid).Append("/").Append(quote.Ask);
            builder.AppendLine();
            PrintBookSide(quote.AskBook, "Ask", true);
            builder.AppendLine();
            PrintBookSide(quote.BidBook, "Bid", false);
            UpdateStatus(builder.ToString());
        }

        private void PrintBookSide(IReadOnlyList<BookEntry> book, string sideName, bool reverse)
        {
            var sortedBook = reverse ? book.Reverse() : book;

            foreach (var entry in sortedBook)
            {
                builder.Append(FormatPrice(entry.Price)).Append(" ")
                    .Append(sideName).Append(" ")
                    .Append(entry.Volume)
                    .AppendLine();
            }
        }

        private string FormatPrice(double price)
        {
            int digits = Symbol.Digits;
            return price.ToString("F" + digits);
        }

        private double VWAPFunc(IReadOnlyList<BookEntry> entry)
        {
            return 0;
        }
    }
}
