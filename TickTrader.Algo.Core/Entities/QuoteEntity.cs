using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Common.Business;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class QuoteEntity : Api.Quote, ISymbolRate
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

        decimal ISymbolRate.Ask => (decimal)Ask;
        decimal ISymbolRate.Bid => (decimal)Bid;
        decimal? ISymbolRate.NullableAsk => double.IsNaN(Ask) ? null : (decimal?)Ask;
        decimal? ISymbolRate.NullableBid => double.IsNaN(Bid) ? null : (decimal?)Bid;

        public override string ToString()
        {
            var bookDepth = System.Math.Max(BidList?.Length ?? 0, AskList?.Length ?? 0);
            return "{ " + Bid + "/" + Ask + " " + Time + " d" + bookDepth + "}";
        }
    }

    [Serializable]
    public class BookEntryEntity : Api.BookEntry
    {
        public double Price { get; set; }
        public double Volume { get; set; }
    }
}
