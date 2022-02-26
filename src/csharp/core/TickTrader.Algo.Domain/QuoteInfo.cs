using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.InteropServices;

namespace TickTrader.Algo.Domain
{
    public readonly struct QuoteBand
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
            : this(time.ToUniversalTime().ToTimestamp(), bid, ask)
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
        private string _symbol;
        private QuoteBand[] _asks, _bids;
        private int? _depth;


        public string Symbol => _symbol;

        public bool HasAsk => _asks.Length != 0;

        public bool HasBid => _bids.Length != 0;

        public bool IsAskIndicative { get; set; }

        public bool IsBidIndicative { get; set; }

        public ReadOnlySpan<QuoteBand> Asks => !_depth.HasValue ? _asks : _asks.AsSpan(0, _depth.Value);

        public ReadOnlySpan<QuoteBand> Bids => !_depth.HasValue ? _bids : _bids.AsSpan(0, _depth.Value);

        public double Ask => HasAsk ? _asks[0].Price : double.NaN;

        public double Bid => HasBid ? _bids[0].Price : double.NaN;

        public DateTime Time => Timestamp.ToDateTime().ToLocalTime();

        public Timestamp Timestamp { get; set; }


        public DateTime TimeOfReceive { get; }

        public double QuoteDelay { get; }


        public QuoteInfo(string symbol) // empty rate
            : this(symbol, null, null, null)
        {
        }

        public QuoteInfo(string symbol, DateTime time, double? bid, double? ask)
            : this(symbol, time.ToUniversalTime().ToTimestamp(), bid, ask)
        {
        }

        public QuoteInfo(string symbol, Timestamp time, double? bid, double? ask)
            : this(symbol, time, bid.HasValue ? new QuoteBand[] { new QuoteBand(bid.Value, 0) } : new QuoteBand[0], ask.HasValue ? new QuoteBand[] { new QuoteBand(ask.Value, 0) } : new QuoteBand[0])
        {
        }

        public QuoteInfo(string symbol, Timestamp time, QuoteBand[] bids, QuoteBand[] asks, int? depth = null, DateTime? timeOfReceive = null)
        {
            _symbol = symbol;
            Timestamp = time;
            _depth = depth;

            _bids = bids ?? new QuoteBand[0];
            _asks = asks ?? new QuoteBand[0];

            TimeOfReceive = timeOfReceive ?? DateTime.UtcNow;
            QuoteDelay = (TimeOfReceive - Time.ToUniversalTime()).TotalMilliseconds;
        }

        public QuoteInfo(FullQuoteInfo quote, int? depth = null)
            : this(quote.Symbol, quote.Data)
        {
            _depth = depth;
        }

        public QuoteInfo(string symbol, QuoteData data)
        {
            _symbol = symbol;
            Timestamp = data.Time;
            IsAskIndicative = data.IsAskIndicative;
            IsBidIndicative = data.IsBidIndicative;
            _asks = UnpackBands(data.AskBytes);
            _bids = UnpackBands(data.BidBytes);
        }

        private QuoteInfo() { }


        public QuoteInfo Truncate(int depth)
        {
            return new QuoteInfo
            {
                _symbol = _symbol,
                Timestamp = Timestamp,
                _depth = depth < 1 ? (int?)null : depth,
                _asks = _asks,
                _bids = _bids,
            };
        }

        public FullQuoteInfo GetFullQuote()
        {
            return new FullQuoteInfo { Symbol = _symbol, Data = GetData() };
        }

        public QuoteData GetData()
        {
            return new QuoteData
            {
                Time = Timestamp,
                IsAskIndicative = IsAskIndicative,
                IsBidIndicative = IsBidIndicative,
                AskBytes = PackBands(_asks),
                BidBytes = PackBands(_bids),
            };
        }

        public string GetDelayInfo()
        {
            return $"{Symbol} Delay={QuoteDelay}ms, ServerTime={Time:dd-MM-yyyy HH:mm:ss.fffff}, ClientTime={TimeOfReceive:dd-MM-yyyy HH:mm:ss.fffff}";
        }


        private static ByteString PackBands(QuoteBand[] bands)
        {
            var byteSpan = MemoryMarshal.Cast<QuoteBand, byte>(bands.AsSpan());
            return ByteStringHelper.CopyFromUglyHack(byteSpan);
        }

        private static QuoteBand[] UnpackBands(ByteString data)
        {
            return MemoryMarshal.Cast<byte, QuoteBand>(data.Span).ToArray();
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
