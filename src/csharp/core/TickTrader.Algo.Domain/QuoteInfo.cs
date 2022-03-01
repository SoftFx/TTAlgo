using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Buffers;
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
        DateTime TimeUtc { get; }
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

    public class QuoteL2Data
    {
        private static readonly byte[] _emptyBands = Array.Empty<byte>();

        private int _bidSize, _askSize;


        public byte[] BidBytes { get; } = _emptyBands;

        public byte[] AskBytes { get; } = _emptyBands;

        public ReadOnlySpan<QuoteBand> Bids => GetBands(BidBytes, _bidSize);

        public ReadOnlySpan<QuoteBand> Asks => GetBands(AskBytes, _askSize);


        public QuoteL2Data(double? bid, double? ask)
        {
            if (bid.HasValue)
            {
                BidBytes = InitTickSide(bid.Value);
                _bidSize = QuoteBand.Size;
            }
            if (ask.HasValue)
            {
                AskBytes = InitTickSide(ask.Value);
                _askSize = QuoteBand.Size;
            }
        }

        public QuoteL2Data(byte[] bids, byte[] asks, int? depth = null)
        {
            if (bids != null && bids.Length != 0)
            {
                BidBytes = bids;
                _bidSize = depth.HasValue ? Math.Min(bids.Length, 16 * depth.Value) : bids.Length;
            }
            if (asks != null && asks.Length != 0)
            {
                AskBytes = asks;
                _askSize = depth.HasValue ? Math.Min(asks.Length, 16 * depth.Value) : asks.Length;
            }
        }


        public QuoteL2Data Truncate(int depth) => new QuoteL2Data(BidBytes, AskBytes, depth);


        private static byte[] InitTickSide(double price)
        {
            var bandBytes = new byte[QuoteBand.Size];
            var data = MemoryMarshal.Cast<byte, double>(bandBytes);
            data[0] = price;
            return bandBytes;
        }

        private static ReadOnlySpan<QuoteBand> GetBands(byte[] bandBytes, int size)
        {
            return MemoryMarshal.Cast<byte, QuoteBand>(bandBytes.AsSpan(0, size));
        }
        //private static ReadOnlySpan<QuoteBand> GetBands(byte[] bandBytes, int? depth)
        //{
        //    var bands = MemoryMarshal.Cast<byte, QuoteBand>(bandBytes);
        //    return depth.HasValue ? bands.Slice(0, depth.Value) : bands;
        //}
    }

    public class QuoteInfo : IQuoteInfo, IRateInfo
    {
        private string _symbol;
        private QuoteL2Data _l2Data;


        public string Symbol => _symbol;

        public double Bid { get; }

        public double Ask { get; }

        public bool HasBid => !double.IsNaN(Bid);

        public bool HasAsk => !double.IsNaN(Ask);

        public bool IsAskIndicative { get; set; }

        public bool IsBidIndicative { get; set; }

        public long UtcTicks { get; private set; }

        public long UtcMs => TimeMs.FromUtcTicks(UtcTicks);

        public DateTime TimeUtc => TimeTicks.ToUtc(UtcTicks);

        public Timestamp Timestamp => TimeTicks.ToTimestamp(UtcTicks);

        public QuoteL2Data L2Data
        {
            get
            {
                if (_l2Data == null)
                    _l2Data = new QuoteL2Data(double.IsNaN(Bid) ? (double?)null : Bid, double.IsNaN(Ask) ? (double?)null : Ask);

                return _l2Data;
            }
        }


        public DateTime TimeOfReceive { get; }

        public double QuoteDelay { get; }


        public QuoteInfo(string symbol, DateTime time, double? bid, double? ask)
            : this(symbol, time.ToUniversalTime().Ticks, bid, ask)
        {
        }

        public QuoteInfo(string symbol, long utcTicks, double? bid, double? ask)
            : this(symbol, utcTicks)
        {
            Bid = bid ?? double.NaN;
            Ask = ask ?? double.NaN;
        }

        public QuoteInfo(string symbol, long utcTicks, byte[] bids, byte[] asks, DateTime? timeOfReceive = null)
            : this(symbol, utcTicks)
        {
            _l2Data = new QuoteL2Data(bids, asks);
            Bid = GetFirstPrice(bids);
            Ask = GetFirstPrice(asks);

            TimeOfReceive = timeOfReceive ?? DateTime.UtcNow;
            QuoteDelay = TimeSpan.FromTicks(TimeOfReceive.Ticks - UtcTicks).TotalMilliseconds;
        }


        private QuoteInfo(string symbol, long utcTicks)
        {
            _symbol = symbol;
            UtcTicks = utcTicks;
        }


        public static QuoteInfo Create(FullQuoteInfo quote) => Create(quote.Symbol, quote.Data);

        public static QuoteInfo Create(string symbol, QuoteData data)
        {
            var time = TimeTicks.FromTimestamp(data.Time);
            QuoteInfo res;

            if (data.TickBytes.IsEmpty)
            {
                var bids = UnpackBands(data.BidBytes);
                var asks = UnpackBands(data.AskBytes);
                res = new QuoteInfo(symbol, time, bids, asks);
            }
            else
            {
                var tick = MemoryMarshal.Cast<byte, double>(data.TickBytes.Span);
                var bid = tick[0];
                var ask = tick[1];
                res = new QuoteInfo(symbol, time, bid, ask);
            }

            res.IsBidIndicative = data.IsBidIndicative;
            res.IsAskIndicative = data.IsAskIndicative;
            return res;
        }


        public QuoteInfo Truncate(int depth)
        {
            QuoteInfo res;

            if (_l2Data == null)
            {
                res=  new QuoteInfo(Symbol, UtcTicks, Bid, Ask);
            }
            else
            {
                res =new QuoteInfo(Symbol, UtcTicks, Bid, Ask)
                {
                    _l2Data = _l2Data.Truncate(depth),
                    IsBidIndicative = IsBidIndicative,
                    IsAskIndicative = IsAskIndicative,
                };
            }

            res.IsBidIndicative = IsBidIndicative;
            res.IsAskIndicative = IsAskIndicative;
            return res;
        }

        public FullQuoteInfo GetFullQuote()
        {
            return new FullQuoteInfo { Symbol = _symbol, Data = GetData() };
        }

        public QuoteData GetData()
        {
            var res = new QuoteData
            {
                Time = Timestamp,
                IsAskIndicative = IsAskIndicative,
                IsBidIndicative = IsBidIndicative,
            };

            if (_l2Data == null)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(2 * sizeof(double));
                try
                {
                    var tick = MemoryMarshal.Cast<byte, double>(buffer);
                    tick[0] = Bid;
                    tick[1] = Ask;
                    res.TickBytes = ByteString.CopyFrom(buffer);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            else
            {
                res.BidBytes = PackBands(_l2Data.BidBytes);
                res.AskBytes = PackBands(_l2Data.AskBytes);
            }

            return res;
        }

        public string GetDelayInfo()
        {
            return $"{Symbol} Delay={QuoteDelay}ms, ServerTime={TimeUtc:dd-MM-yyyy HH:mm:ss.fffff}, ClientTime={TimeOfReceive:dd-MM-yyyy HH:mm:ss.fffff}";
        }


        private static ByteString PackBands(byte[] bands)
        {
            return ByteString.CopyFrom(bands);
        }

        private static byte[] UnpackBands(ByteString data)
        {
            return data.IsEmpty ? Array.Empty<byte>() : data.ToByteArray();
        }

        private static double GetFirstPrice(byte[] bandBytes)
        {
            if (bandBytes.Length == 0)
                return double.NaN;

            var data = MemoryMarshal.Cast<byte, double>(bandBytes);
            return data[0];
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
