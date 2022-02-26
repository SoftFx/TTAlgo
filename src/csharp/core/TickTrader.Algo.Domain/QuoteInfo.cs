using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.InteropServices;

namespace TickTrader.Algo.Domain
{
    public readonly struct QuoteBand
    {
        public const int Size = 16;


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
        private static readonly byte[] _emptyBands = new byte[0];

        private string _symbol;
        private byte[] _askBytes, _bidBytes;
        private int? _depth;


        public string Symbol => _symbol;

        public bool HasAsk => _askBytes.Length != 0;

        public bool HasBid => _bidBytes.Length != 0;

        public bool IsAskIndicative { get; set; }

        public bool IsBidIndicative { get; set; }

        public byte[] AskBytes => _askBytes;

        public byte[] BidBytes => _bidBytes;

        public ReadOnlySpan<QuoteBand> Asks => GetBands(_askBytes, _depth);

        public ReadOnlySpan<QuoteBand> Bids => GetBands(_bidBytes, _depth);

        public double Ask => GetFirstPrice(_askBytes);

        public double Bid => GetFirstPrice(_bidBytes);

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
            : this(symbol, time)
        {
            if (bid.HasValue)
                _bidBytes = InitTickSide(bid.Value);
            if (ask.HasValue)
                _askBytes = InitTickSide(ask.Value);
        }

        public QuoteInfo(string symbol, Timestamp time, byte[] bids, byte[] asks, int? depth = null, DateTime? timeOfReceive = null)
            : this(symbol, time, depth)
        {
            _bidBytes = bids ?? _emptyBands;
            _askBytes = asks ?? _emptyBands;

            TimeOfReceive = timeOfReceive ?? DateTime.UtcNow;
            QuoteDelay = (TimeOfReceive - Time.ToUniversalTime()).TotalMilliseconds;
        }

        public QuoteInfo(FullQuoteInfo quote, int? depth = null)
            : this(quote.Symbol, quote.Data)
        {
            _depth = depth;
        }

        public QuoteInfo(string symbol, QuoteData data)
            : this(symbol, data.Time)
        {
            IsBidIndicative = data.IsBidIndicative;
            IsAskIndicative = data.IsAskIndicative;
            _bidBytes = UnpackBands(data.BidBytes);
            _askBytes = UnpackBands(data.AskBytes);
        }

        private QuoteInfo() { }

        private QuoteInfo(string symbol, Timestamp time, int? depth = null)
        {
            _symbol = symbol;
            Timestamp = time;
            _depth = depth;

            _bidBytes = _emptyBands;
            _askBytes = _emptyBands;
        }


        public QuoteInfo Truncate(int depth)
        {
            return new QuoteInfo
            {
                _symbol = _symbol,
                Timestamp = Timestamp,
                _depth = depth < 1 ? (int?)null : depth,
                _askBytes = _askBytes,
                _bidBytes = _bidBytes,
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
                AskBytes = PackBands(_askBytes),
                BidBytes = PackBands(_bidBytes),
            };
        }

        public string GetDelayInfo()
        {
            return $"{Symbol} Delay={QuoteDelay}ms, ServerTime={Time:dd-MM-yyyy HH:mm:ss.fffff}, ClientTime={TimeOfReceive:dd-MM-yyyy HH:mm:ss.fffff}";
        }


        private static ByteString PackBands(byte[] bands)
        {
            return ByteString.CopyFrom(bands);
        }

        private static byte[] UnpackBands(ByteString data)
        {
            return data.ToByteArray();
        }

        private static ReadOnlySpan<QuoteBand> GetBands(byte[] bandBytes, int? depth)
        {
            var bands = MemoryMarshal.Cast<byte, QuoteBand>(bandBytes);
            return depth.HasValue ? bands.Slice(0, depth.Value) : bands;
        }

        private static double GetFirstPrice(byte[] bandBytes)
        {
            if (bandBytes.Length == 0)
                return double.NaN;

            var data = MemoryMarshal.Cast<byte, double>(bandBytes);
            return data[0];
        }

        private static byte[] InitTickSide(double price)
        {
            var bandBytes = new byte[QuoteBand.Size];
            var data = MemoryMarshal.Cast<byte, double>(bandBytes);
            data[0] = price;
            return bandBytes;
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
