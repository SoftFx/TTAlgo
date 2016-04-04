using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class Bar : Api.Bar
    {
        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Volume { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
    }

    [Serializable]
    public struct Quote
    {
        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public List<BookEntry> AskBook { get; set; }
        public List<BookEntry> BidBook { get; set; }
    }

    [Serializable]
    public class BookEntry
    {
        public decimal Price { get; set; }
        public decimal Value { get; set; }
    }
}
