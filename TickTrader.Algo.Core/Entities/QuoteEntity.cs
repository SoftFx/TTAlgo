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
    public class QuoteEntity : Api.Quote, ISymbolRate, RateUpdate
    {
        public static readonly BookEntry[] EmptyBook = new BookEntry[0];

        public QuoteEntity(string symbol, DateTime time, double bid, double ask)
            : this(symbol, time, new BookEntry(bid, 0), new BookEntry(ask, 0))
        {
        }

        public QuoteEntity(string symbol, DateTime time, BookEntry bid, BookEntry ask)
            : this(symbol, time, new BookEntry[] { bid }, new BookEntry[] { ask })
        {
        }

        public QuoteEntity(string symbol, DateTime time, double? bid, double? ask)
            : this(symbol, time, bid == null ? null : new BookEntry[] { new BookEntry(bid.Value, 0) }, ask == null ? null : new BookEntry[] { new BookEntry(ask.Value, 0) })
        {
        }

        public QuoteEntity(string symbol, DateTime time, BookEntry[] bids, BookEntry[] asks)
        {
            Symbol = symbol;
            Time = time;
            
            bids = bids ?? new BookEntry[0];
            asks = asks ?? new BookEntry[0];

            Array.Sort(bids, (x, y) => y.Price.CompareTo(x.Price));
            Array.Sort(asks, (x, y) => x.Price.CompareTo(y.Price));

            BidList = bids;
            AskList = asks;

            if (bids.Length > 0)
                Bid = bids[0].Price;
            else
                Bid = double.NaN;

            if (asks.Length > 0)
                Ask = asks[0].Price;
            else
                Ask = double.NaN;
        }

        private QuoteEntity()
        {
        }

        public static QuoteEntity CreatePrepared(string symbol, DateTime time, BookEntry[] bids, BookEntry[] asks)
        {
            var entity = new QuoteEntity();

            entity.BidList = bids;
            entity.AskList = asks;
            entity.Time = time;
            entity.Symbol = symbol;

            if (bids.Length > 0)
                entity.Bid = bids[0].Price;
            else
                entity.Bid = double.NaN;

            if (asks.Length > 0)
                entity.Ask = asks[0].Price;
            else
                entity.Ask = double.NaN;

            return entity;
        }

        public string Symbol { get; set; }
        public DateTime Time { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }

        public BookEntry[] BidList { get; private set; }
        public BookEntry[] AskList { get; private set; }

        public BookEntry[] BidBook { get { return BidList; } }
        public BookEntry[] AskBook { get { return AskList; } }

        decimal ISymbolRate.Ask => (decimal)Ask;
        decimal ISymbolRate.Bid => (decimal)Bid;
        decimal? ISymbolRate.NullableAsk => double.IsNaN(Ask) ? null : (decimal?)Ask;
        decimal? ISymbolRate.NullableBid => double.IsNaN(Bid) ? null : (decimal?)Bid;
        bool ISymbolRate.IndicativeTick => throw new NotImplementedException();

        #region RateUpdate

        double RateUpdate.AskHigh => Ask;
        double RateUpdate.AskLow => Ask;
        double RateUpdate.AskOpen => Ask;
        double RateUpdate.BidHigh => Bid;
        double RateUpdate.BidLow => Bid;
        double RateUpdate.BidOpen => Bid;
        int RateUpdate.NumberOfQuotes => 1;
        
        Quote RateUpdate.LastQuote => this;

        #endregion

        #region FDK compatibility

        public DateTime CreatingTime => Time;
        public bool HasBid => !double.IsNaN(Bid);
        public bool HasAsk => !double.IsNaN(Ask);

        public double? GetNullableBid()
        {
            return double.IsNaN(Bid) ? null : (double?)Bid;
        }

        public double? GetNullableAsk()
        {
            return double.IsNaN(Ask) ? null : (double?)Ask;
        }

        #endregion

        public override string ToString()
        {
            var bookDepth = System.Math.Max(BidList?.Length ?? 0, AskList?.Length ?? 0);
            return "{ " + Bid + "/" + Ask + " " + Time + " d" + bookDepth + "}";
        }
    }

    //[Serializable]
    //public class BookEntryEntity : Api.BookEntry
    //{
    //    public BookEntryEntity()
    //    {
    //    }

    //    public BookEntryEntity(double price, double volume = 0)
    //    {
    //        Price = price;
    //        Volume = volume;
    //    }

    //    public double Price { get; set; }
    //    public double Volume { get; set; }
    //}
}
