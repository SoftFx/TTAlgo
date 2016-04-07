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
        public static readonly BarEntity Empty = new BarEntity() { IsEmpty = true, Open = double.NaN, Close = double.NaN, High = double.NaN , Low = double.NaN, Volume = double.NaN };

        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Volume { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public bool IsEmpty { get; set; }
    }

    [Serializable]
    public class QuoteEntity : Api.Quote
    {
        public DateTime Time { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }
    }

    [Serializable]
    public class Level2QuoteEntity : QuoteEntity, Api.Level2Quote
    {
        public List<BookEntryEntity> AskBook { get; set; }
        public List<BookEntryEntity> BidBook { get; set; }

        IReadOnlyList<BookEntry> Level2Quote.AskBook { get { return AskBook; } }
        IReadOnlyList<BookEntry> Level2Quote.BidBook { get { return BidBook; } }
    }

    [Serializable]
    public class BookEntryEntity : Api.BookEntry
    {
        public double Price { get; set; }
        public double Value { get; set; }
    }
}
