using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot2
{
    internal class Edge
    {
        private string symbol;

        public Edge(string symbol, PriceObserver observer)
        {
            this.symbol = symbol;
            this.Observer = observer;
        }

        public PriceObserver Observer { get; private set; }
        public bool IsReversed { get; private set; }
        public bool HasPrice { get { return Observer.HasPrice; } }

        /// <summary>
        /// Return vwap price for requested volume or if it is negatice coef to reduce requested volume
        /// </summary>
        /// <param name="requiredVolume"></param>
        /// <returns></returns>
        public double GetPriceForVolume(double requiredVolume, OrderSide side)
        {
            if (side == OrderSide.Buy)
                return GetPriceForVolume(requiredVolume, Observer.AskBook);
            else
                return GetPriceForVolume(requiredVolume, Observer.BidBook);
        }

        private double GetPriceForVolume(double requiredVolume, BookEntry[] book)
        {
            if (IsReversed)
            {
                var reversedEntries = book.Reverse().Select(e => new BookEntryEntity(1 / e.Price, e.Volume * e.Price));
                return GetPriceForVolumeInBook(requiredVolume, reversedEntries);
            }
            else
                return GetPriceForVolumeInBook(requiredVolume, book);
        }

        private double GetPriceForVolumeInBook(double requiredVolume, IEnumerable<BookEntry> entries)
        {
            double leftVolume = requiredVolume;
            double numerator = 0;
            foreach (BookEntry currEntry in entries)
            {
                if (leftVolume >= currEntry.Volume)
                {
                    leftVolume -= currEntry.Volume;
                    numerator += currEntry.Volume * currEntry.Price;
                }
                else
                {
                    numerator += leftVolume * currEntry.Price;
                    leftVolume = 0;
                }
            }
            if (leftVolume > float.Epsilon)
                return leftVolume / requiredVolume - 1;
            return numerator / requiredVolume;
        }

        public string PrintAsk()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(symbol).Append(": ");
            if (Observer.HasPrice)
                PrintBook(Observer.AskBook, builder);
            else
                builder.Append("OffQuotes");
            return builder.ToString();
        }

        public string PrintBid()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(symbol).Append(": ");
            if (Observer.HasPrice)
                PrintBook(Observer.BidBook, builder);
            else
                builder.Append("OffQuotes");
            return builder.ToString();
        }

        private void PrintBook(BookEntry[] book, StringBuilder builder)
        {
            for (int i = 0; i < book.Length; i++)
            {
                if (i > 0)
                    builder.Append('|');
                builder.Append(book[i].Price).Append(" ").Append(book[i].Volume);
            }
        }
    }
}
