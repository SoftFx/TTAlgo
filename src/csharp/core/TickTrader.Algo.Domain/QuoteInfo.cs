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


        public QuoteData(DateTime time, double? bid, double? ask)
            : this(time.ToUniversalTime().ToTimestamp(), bid,ask)
        {
        }

        public QuoteData(Timestamp time, double? bid, double? ask)
        {
            Time = time;
            SetBids(bid.HasValue ? new QuoteBand[] { new QuoteBand(bid.Value, 0) } : new QuoteBand[0]);
            SetAsks(ask.HasValue ? new QuoteBand[] { new QuoteBand(ask.Value, 0) } : new QuoteBand[0]);
        }


        public void SetAsks(QuoteBand[] bands)
        {
            var byteSpan = MemoryMarshal.Cast<QuoteBand, byte>(bands.AsSpan());
            AskBytes = ByteStringHelper.CopyFromUglyHack(byteSpan);
        }

        public void SetBids(QuoteBand[] bands)
        {
            var byteSpan = MemoryMarshal.Cast<QuoteBand, byte>(bands.AsSpan());
            BidBytes = ByteStringHelper.CopyFromUglyHack(byteSpan);
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
        Timestamp Timestamp { get; }
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
        // hack to bypass net45 code security
        // TODO: Remove after swiching to .NET Core
        private byte[] _askBytes;
        private byte[] _bidBytes;

        private string _symbol;
        private QuoteData _data;
        private int? _depth;


        public string Symbol => _symbol;

        public ReadOnlySpan<QuoteBand> Asks
        {
            get
            {
                //var asks = _data.Asks;
                var asks = MemoryMarshal.Cast<byte, QuoteBand>(_askBytes);
                return _depth.HasValue ? asks.Slice(0, Math.Min(asks.Length, _depth.Value)) : asks;
            }
        }

        public ReadOnlySpan<byte> AskBytes => _askBytes;

        public ReadOnlySpan<QuoteBand> Bids
        {
            get
            {
                //var bids = _data.Bids;
                var bids = MemoryMarshal.Cast<byte, QuoteBand>(_bidBytes);
                return _depth.HasValue ? bids.Slice(0, Math.Min(bids.Length, _depth.Value)) : bids;
            }
        }

        public ReadOnlySpan<byte> BidBytes => _bidBytes;

        public bool HasAsk => _data.HasAsk;

        public bool HasBid => _data.HasBid;

        public bool IsAskIndicative => _data.IsAskIndicative;

        public bool IsBidIndicative => _data.IsBidIndicative;

        //public double Ask => HasAsk ? _data.Asks[0].Price : double.NaN;
        public double Ask => HasAsk ? Asks[0].Price : double.NaN;

        public double Bid => HasBid ? Bids[0].Price : double.NaN;
        //public double Bid => HasBid ? _data.Bids[0].Price : double.NaN;

        public DateTime Time => _data.Time.ToDateTime().ToLocalTime();

        public Timestamp Timestamp => _data.Time;


        public QuoteInfo(string symbol) // empty rate
            : this(symbol, new QuoteData(), null)
        {
        }

        public QuoteInfo(string symbol, DateTime time, double? bid, double? ask)
            : this(symbol, new QuoteData(time, bid, ask), null)
        {
        }

        public QuoteInfo(string symbol, Timestamp time, double? bid, double? ask)
            : this(symbol, new QuoteData(time, bid, ask), null)
        {
        }

        public QuoteInfo(FullQuoteInfo quote, int? depth = null)
            : this(quote.Symbol, quote.Data, depth)
        {
        }

        public QuoteInfo(string symbol, QuoteData data, int? depth = null)
        {
            _symbol = symbol;
            _data = data;
            _depth = depth;

            _bidBytes = _data.BidBytes.ToByteArray();
            _askBytes = _data.AskBytes.ToByteArray();
        }

        private QuoteInfo() { }


        public QuoteInfo Truncate(int depth)
        {
            return new QuoteInfo
            {
                _symbol = Symbol,
                _data = _data,
                _depth = depth < 1 ? (int?)null : depth,
                _askBytes = _askBytes,
                _bidBytes = _bidBytes,
            };
        }

        public FullQuoteInfo GetFullQuote()
        {
            return new FullQuoteInfo { Symbol = _symbol, Data = _data };
        }

        public QuoteData GetData()
        {
            return _data;
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
