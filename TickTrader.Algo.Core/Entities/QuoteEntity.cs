using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class QuoteEntity : Api.Quote, RateUpdate
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

        #region RateUpdate

        double RateUpdate.AskHigh => Ask;
        double RateUpdate.AskLow => Ask;
        double RateUpdate.BidHigh => Bid;
        double RateUpdate.BidLow => Bid;
        double RateUpdate.NumberOfQuotes => 1;
        Quote RateUpdate.LastQuote => this;

        #endregion

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
