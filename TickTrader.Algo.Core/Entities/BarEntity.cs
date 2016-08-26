using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class BarEntity : Api.Bar
    {
        public static readonly BarEntity Empty = new BarEntity() { IsNull = true, Open = double.NaN, Close = double.NaN, High = double.NaN , Low = double.NaN, Volume = double.NaN };

        public BarEntity()
        {
        }

        public BarEntity(DateTime openTime, DateTime closeTime, QuoteEntity quote)
        {
            OpenTime = openTime;
            CloseTime = closeTime;
            Open = quote.Bid;
            Close = quote.Bid;
            High = quote.Bid;
            Low = quote.Bid;
            Volume = 1;
        }

        public BarEntity(BarEntity original)
        {
            OpenTime = original.OpenTime;
            CloseTime = original.CloseTime;
            Open = original.Open;
            Close = original.Close;
            High = original.High;
            Low = original.Low;
        }

        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Volume { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public bool IsNull { get; set; }

        public BarEntity Clone()
        {
            return new BarEntity(this);
        }

        public void Append(double price)
        {
            Close = price;
            if (price > High)
                High = price;
            if (price < Low)
                Low = price;
            Volume++;
        }

        public BarEntity CopyAndAppend(double price)
        {
            var clone = Clone();
            clone.Append(price);
            return clone;
        }
    }

    [Serializable]
    public class QuoteEntity : Api.Quote
    {
        public static readonly IReadOnlyList<BookEntry> EmptyBook = new List<BookEntry>().AsReadOnly();

        public string SymbolCode { get; set; }
        public DateTime Time { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }

        public IReadOnlyList<BookEntry> BidList { get; set; }
        public IReadOnlyList<BookEntry> AskList { get; set; }

        public IReadOnlyList<BookEntry> BidBook { get { return BidList; } }
        public IReadOnlyList<BookEntry> AskBook { get { return AskList; } }
    }

    [Serializable]
    public class BookEntryEntity : Api.BookEntry
    {
        public double Price { get; set; }
        public double Volume { get; set; }
    }
}
