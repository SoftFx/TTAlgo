using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;
using TickTrader.Common.Business;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class QuoteEntity : Api.Quote, ISymbolRate, RateUpdate
    {
        public static readonly BookEntry[] EmptyBook = new BookEntry[0];

        private double? _ask;
        private double? _bid;

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

        // Empty quote can't be indicative. Indicative quote should always have price
        public QuoteEntity(string symbol, DateTime time, BookEntry[] bids, BookEntry[] asks)
            : this(symbol, time, bids, asks, bids?.Length > 0 ? bids[0].Volume <= 0 : false, asks?.Length > 0 ? asks[0].Volume <= 0 : false)
        {
        }

        public QuoteEntity(string symbol, DateTime time, BookEntry[] bids, BookEntry[] asks, bool isBidIndicative, bool isAskIndicative)
        {
            Symbol = symbol;
            Time = time;
            IsBidIndicative = isBidIndicative;
            IsAskIndicative = isAskIndicative;

            bids = bids ?? new BookEntry[0];
            asks = asks ?? new BookEntry[0];

            Array.Sort(bids, (x, y) => y.Price.CompareTo(x.Price));
            Array.Sort(asks, (x, y) => x.Price.CompareTo(y.Price));

            BidList = bids;
            AskList = asks;

            if (bids.Length > 0)
                _bid = bids[0].Price;
            else
                _bid = null;

            if (asks.Length > 0)
                _ask = asks[0].Price;
            else
                _ask = null;
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
                entity._bid = bids[0].Price;
            else
                entity._bid = null;

            if (asks.Length > 0)
                entity._ask = asks[0].Price;
            else
                entity._ask = null;

            return entity;
        }

        public string Symbol { get; set; }
        public DateTime Time { get; set; }
        public double Ask => _ask ?? double.NaN;
        public double Bid => _bid ?? double.NaN;
        public BookEntry[] BidList { get; private set; }
        public BookEntry[] AskList { get; private set; }
        public bool IsAskIndicative { get; private set; }
        public bool IsBidIndicative { get; private set; }

        public BookEntry[] BidBook { get { return BidList; } }
        public BookEntry[] AskBook { get { return AskList; } }

        decimal ISymbolRate.Ask => (decimal)Ask;
        decimal ISymbolRate.Bid => (decimal)Bid;
        public decimal? NullableAsk => (decimal?)_ask;
        public decimal? NullableBid => (decimal?)_bid;
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
        public bool HasBid => _bid.HasValue;
        public bool HasAsk => _ask.HasValue;

        public double? GetNullableBid() => _bid;
        public double? GetNullableAsk() => _ask;

        #endregion

        public override string ToString()
        {
            var bookDepth = System.Math.Max(BidList?.Length ?? 0, AskList?.Length ?? 0);
            return $"{{{Bid}{(IsBidIndicative ? "i" : "")}/{Ask}{(IsAskIndicative ? "i" : "")} {Time} d{bookDepth}}}";
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
