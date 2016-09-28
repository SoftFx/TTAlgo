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

        public BarEntity(DateTime openTime, DateTime closeTime, double price, double volume)
        {
            OpenTime = openTime;
            CloseTime = closeTime;
            Open = price;
            Close = price;
            High = price;
            Low = price;
            Volume = volume;
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

        public void Append(double price, double volume)
        {
            Close = price;
            if (price > High)
                High = price;
            if (price < Low)
                Low = price;
            Volume += volume;
        }

        public BarEntity CopyAndAppend(double price, double volume)
        {
            var clone = Clone();
            clone.Append(price, volume);
            return clone;
        }
    }

    [Serializable]
    public class QuoteEntity : Api.Quote
    {
        public static readonly BookEntry[] EmptyBook = new BookEntry[0];

        public string Symbol { get; set; }
        public DateTime Time { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }

        public BookEntry[] BidList { get; set; }
        public BookEntry[] AskList { get; set; }

        public BookEntry[] BidBook { get { return BidList; } }
        public BookEntry[] AskBook { get { return AskList; } }
    }

    [Serializable]
    public class BookEntryEntity : Api.BookEntry
    {
        public double Price { get; set; }
        public double Volume { get; set; }
    }
}
