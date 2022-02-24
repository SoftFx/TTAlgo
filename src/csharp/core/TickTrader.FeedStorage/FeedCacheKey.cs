using System;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    public class FeedCacheKey : ISeriesKey
    {
        private static readonly char[] _codeSeparators = new char[] { '_' };


        public string Symbol { get; }

        public Feed.Types.Timeframe TimeFrame { get; }

        public Feed.Types.MarketSide? MarketSide { get; }

        public string FullInfo => $"{Symbol}_{TimeFrame}_{MarketSide?.ToString() ?? string.Empty}";


        public FeedCacheKey(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide? priceType = null)
        {
            Symbol = symbol;
            TimeFrame = timeframe;
            MarketSide = TimeFrame.IsTick() ? null : priceType; //better MarketSize = priceType, fool-proof
        }

        public FeedCacheKey(ISeriesKey key) : this(key.Symbol, key.TimeFrame, key.MarketSide)
        { }


        internal static bool TryParse(string strCode, out FeedCacheKey key)
        {
            var parts = strCode?.Split(_codeSeparators, StringSplitOptions.RemoveEmptyEntries);
            key = null;

            if (parts == null || (parts.Length != 2 && parts.Length != 3))
                return false;

            var result = !string.IsNullOrEmpty(parts[0]) & TryParseStrToEnum(parts[1], out Feed.Types.Timeframe timeframe);

            if (result)
            {
                if (parts.Length == 3)
                {
                    if (TryParseStrToEnum(parts[2], out Feed.Types.MarketSide side))
                        key = new FeedCacheKey(parts[0], timeframe, side);
                    else
                        return false;
                }
                else
                    key = new FeedCacheKey(parts[0], timeframe, null);
            }

            return result;
        }

        private static bool TryParseStrToEnum<TEnum>(string str, out TEnum value) where TEnum : struct
        {
            value = default;
            return !string.IsNullOrEmpty(str) && Enum.TryParse(str, true, out value);
        }

        public override bool Equals(object obj)
        {
            return obj is FeedCacheKey other &&
                   string.Equals(other.Symbol, Symbol) &&
                   other.MarketSide == MarketSide &&
                   other.TimeFrame == TimeFrame;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Symbol.GetHashCode();
                hash = (hash * 16777619) ^ MarketSide.GetHashCode();
                hash = (hash * 16777619) ^ TimeFrame.GetHashCode();
                return hash;
            }
        }
    }
}
