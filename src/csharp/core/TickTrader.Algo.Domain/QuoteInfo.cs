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

    public interface IQuoteInfo
    {
        string Symbol { get; }
        DateTime Time { get; }
        DateTime TimeUtc { get; }
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
        long UtcMs { get; }
        long UtcTicks { get; }
        DateTime Time { get; }
        DateTime TimeUtc { get; }
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
        private static readonly byte[] _emptyBands = Array.Empty<byte>();

        private string _symbol;
        private int? _depth;


        public string Symbol => _symbol;

        public bool HasAsk => AskBytes.Length != 0;

        public bool HasBid => BidBytes.Length != 0;

        public bool IsAskIndicative { get; set; }

        public bool IsBidIndicative { get; set; }

        public byte[] AskBytes { get; private set; }

        public byte[] BidBytes { get; private set; }

        public ReadOnlySpan<QuoteBand> Asks => GetBands(AskBytes, _depth);

        public ReadOnlySpan<QuoteBand> Bids => GetBands(BidBytes, _depth);

        public double Ask => GetFirstPrice(AskBytes);

        public double Bid => GetFirstPrice(BidBytes);

        public long UtcTicks { get; private set; }

        public long UtcMs => TimeMs.FromUtcTicks(UtcTicks);

        public DateTime Time => TimeTicks.ToUtc(UtcTicks).ToLocalTime();

        public DateTime TimeUtc => TimeTicks.ToUtc(UtcTicks);

        public Timestamp Timestamp => TimeTicks.ToTimestamp(UtcTicks);


        public DateTime TimeOfReceive { get; }

        public double QuoteDelay { get; }


        public QuoteInfo(string symbol) // empty rate
            : this(symbol, 0, null, null)
        {
        }

        public QuoteInfo(string symbol, DateTime time, double? bid, double? ask)
            : this(symbol, time.ToUniversalTime().Ticks, bid, ask)
        {
        }

        public QuoteInfo(string symbol, long utcTicks, double? bid, double? ask)
            : this(symbol, utcTicks)
        {
            if (bid.HasValue)
                BidBytes = InitTickSide(bid.Value);
            if (ask.HasValue)
                AskBytes = InitTickSide(ask.Value);
        }

        public QuoteInfo(string symbol, long utcTicks, byte[] bids, byte[] asks, int? depth = null, DateTime? timeOfReceive = null)
            : this(symbol, utcTicks, depth)
        {
            BidBytes = bids ?? _emptyBands;
            AskBytes = asks ?? _emptyBands;

            TimeOfReceive = timeOfReceive ?? DateTime.UtcNow;
            QuoteDelay = TimeSpan.FromTicks(TimeOfReceive.Ticks - UtcTicks).TotalMilliseconds;
        }

        public QuoteInfo(FullQuoteInfo quote, int? depth = null)
            : this(quote.Symbol, quote.Data)
        {
            _depth = depth;
        }

        public QuoteInfo(string symbol, QuoteData data)
            : this(symbol, TimeTicks.FromTimestamp(data.Time))
        {
            IsBidIndicative = data.IsBidIndicative;
            IsAskIndicative = data.IsAskIndicative;
            BidBytes = UnpackBands(data.BidBytes);
            AskBytes = UnpackBands(data.AskBytes);
        }

        private QuoteInfo() { }

        private QuoteInfo(string symbol, long utcTicks, int? depth = null)
        {
            _symbol = symbol;
            UtcTicks = utcTicks;
            _depth = depth;

            BidBytes = _emptyBands;
            AskBytes = _emptyBands;
        }


        public QuoteInfo Truncate(int depth)
        {
            return new QuoteInfo
            {
                _symbol = _symbol,
                UtcTicks = UtcTicks,
                _depth = depth < 1 ? (int?)null : depth,
                AskBytes = AskBytes,
                BidBytes = BidBytes,
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
                AskBytes = PackBands(AskBytes),
                BidBytes = PackBands(BidBytes),
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
