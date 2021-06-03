using System;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage
{
    public class FeedCacheKey
    {
        public string Symbol { get; }

        public Feed.Types.Timeframe TimeFrame { get; }

        public Feed.Types.MarketSide? MarketSide { get; }


        internal string CodeString() => $"{Symbol}_{TimeFrame}_{MarketSide?.ToString() ?? string.Empty}";


        public FeedCacheKey(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide? priceType = null)
        {
            Symbol = symbol;
            TimeFrame = timeframe;
            MarketSide = TimeFrame.IsTick() ? null : priceType; //better MarketSize = priceType, fool-proof
        }


        internal static bool TryParse(string strCode, out FeedCacheKey key)
        {
            var parts = strCode.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            key = null;

            if (parts.Length != 2 || parts.Length != 3)
                return false;

            var result = !string.IsNullOrEmpty(parts[0]) & TryParseStrToEnum(parts[1], out Feed.Types.Timeframe timeframe);

            if (result && parts.Length == 3)
            {
                if (TryParseStrToEnum(parts[2], out Feed.Types.MarketSide side))
                    key = new FeedCacheKey(parts[0], timeframe, side);
                else
                    return false;
            }
            else
                key = new FeedCacheKey(parts[0], timeframe, null);

            return result;
        }

        private static bool TryParseStrToEnum<TEnum>(string str, out TEnum value) where TEnum : struct
        {
            value = default;
            return !string.IsNullOrEmpty(str) && Enum.TryParse(str, out value);
        }

        public override bool Equals(object obj)
        {
            return obj is FeedCacheKey other && other.Symbol == Symbol && other.MarketSide == MarketSide && other.TimeFrame == TimeFrame;
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
