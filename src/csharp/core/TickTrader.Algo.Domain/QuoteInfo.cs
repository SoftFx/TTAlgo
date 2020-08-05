using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.InteropServices;

namespace TickTrader.Algo.Domain
{
    public struct QuoteBand
    {
        public double Price { get; }

        public double Amount { get; }


        public QuoteBand(double price, double amount)
        {
            Price = price;
            Amount = amount;
        }
    }


    public partial class QuoteData
    {
        public ReadOnlySpan<QuoteBand> Asks => MemoryMarshal.Cast<byte, QuoteBand>(AskBytes.Span);

        public ReadOnlySpan<QuoteBand> Bids => MemoryMarshal.Cast<byte, QuoteBand>(BidBytes.Span);

        public bool HasAsk => AskBytes.Length > 0;

        public bool HasBid => BidBytes.Length > 0;


        public QuoteData(DateTime time, double bid, double ask)
        {
            Time = time.ToUniversalTime().ToTimestamp();
            SetBids(new QuoteBand[] { new QuoteBand(bid, 0) });
            SetAsks(new QuoteBand[] { new QuoteBand(ask, 0) });
        }


        public void SetAsks(QuoteBand[] bands)
        {
            var byteSpan = MemoryMarshal.Cast<QuoteBand, byte>(bands.AsSpan());
            AskBytes = ByteString.CopyFrom(byteSpan);
        }

        public void SetBids(QuoteBand[] bands)
        {
            var byteSpan = MemoryMarshal.Cast<QuoteBand, byte>(bands.AsSpan());
            BidBytes = ByteString.CopyFrom(byteSpan);
        }
    }

    public interface IQuoteInfo
    {
        string Symbol { get; }
        DateTime Time { get; }
        double Ask { get; }
        double Bid { get; }

        bool HasAsk { get; }
        bool HasBid { get; }

        bool IsAskIndicative { get; }
        bool IsBidIndicative { get; }
    }

    public interface IRateInfo
    {
        string Symbol { get; }
        DateTime Time { get; }
        bool HasAsk { get; }
        bool HasBid { get; }
        double Ask { get; }
        double AskHigh { get; }
        double AskLow { get; }
        double AskOpen { get; }
        double Bid { get; }
        double BidHigh { get; }
        double BidLow { get; }
        double BidOpen { get; }
        int NumberOfQuotes { get; }

        QuoteInfo LastQuote { get; }
    }

    public class QuoteInfo : IQuoteInfo, IRateInfo
    {
        public string Symbol { get; }

        public QuoteData Data { get; }

        public int? Depth { get; }

        public ReadOnlySpan<QuoteBand> Asks
        {
            get
            {
                var asks = Data.Asks;
                return Depth.HasValue ? asks.Slice(0, Math.Min(asks.Length, Depth.Value)) : asks;
            }
        }

        public ReadOnlySpan<QuoteBand> Bids
        {
            get
            {
                var bids = Data.Bids;
                return Depth.HasValue ? bids.Slice(0, Math.Min(bids.Length, Depth.Value)) : bids;
            }
        }

        public bool HasAsk => Data.HasAsk;

        public bool HasBid => Data.HasBid;

        public bool IsAskIndicative => Data.IsAskIndicative;

        public bool IsBidIndicative => Data.IsBidIndicative;

        public double Ask => HasAsk ? Data.Asks[0].Price : double.NaN;

        public double Bid => HasBid ? Data.Bids[0].Price : double.NaN;

        public DateTime Time => Data.Time.ToDateTime();


        public QuoteInfo(string symbol) // empty rate
            : this(symbol, new QuoteData(), null)
        {
        }

        public QuoteInfo(string symbol, DateTime time, double bid, double ask)
            : this(symbol, new QuoteData(time, bid, ask), null)
        {
        }

        public QuoteInfo(FullQuoteInfo quote, int? depth = null)
            : this(quote.Symbol, quote.Data, depth)
        {
        }

        public QuoteInfo(string symbol, QuoteData data, int? depth = null)
        {
            Symbol = symbol;
            Data = data;
            Depth = depth;
        }


        public QuoteInfo Truncate(int depth)
        {
            return new QuoteInfo(Symbol, Data, depth < 1 ? (int?)null : depth);
        }


        #region IRateInfo

        double IRateInfo.AskHigh => Ask;
        double IRateInfo.AskLow => Ask;
        double IRateInfo.AskOpen => Ask;
        double IRateInfo.BidHigh => Bid;
        double IRateInfo.BidLow => Bid;
        double IRateInfo.BidOpen => Bid;
        int IRateInfo.NumberOfQuotes => 1;

        QuoteInfo IRateInfo.LastQuote => this;

        #endregion
    }
}
