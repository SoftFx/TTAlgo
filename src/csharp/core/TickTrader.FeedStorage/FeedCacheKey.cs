using System;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    public class FeedCacheKey : ISeriesKey
    {
        private const char Separator = '_';

        public string Symbol { get; }

        public string FullInfo { get; }

        public Feed.Types.Timeframe TimeFrame { get; }

        public Feed.Types.MarketSide? MarketSide { get; }

        public SymbolConfig.Types.SymbolOrigin Origin { get; }


        public FeedCacheKey(string symbol, Feed.Types.Timeframe timeframe, SymbolConfig.Types.SymbolOrigin origin = SymbolConfig.Types.SymbolOrigin.Online, Feed.Types.MarketSide? priceType = null)
        {
            Symbol = symbol;
            TimeFrame = timeframe;
            Origin = origin;
            MarketSide = TimeFrame.IsTick() ? null : priceType; //better MarketSize = priceType, fool-proof

            FullInfo = string.Join($"{Separator}", Symbol, Origin, TimeFrame);

            if (MarketSide.HasValue)
                FullInfo += $"{Separator}{MarketSide.Value}";
        }

        public FeedCacheKey(FeedCacheKey other, Feed.Types.MarketSide? priceType = null)
            : this(other.Symbol, other.TimeFrame, other.Origin, priceType)
        {
        }


        public static bool TryParse(string strCode, out FeedCacheKey key)
        {
            key = null;

            if (string.IsNullOrEmpty(strCode))
                return false;

            var parts = strCode.Split(Separator);

            if (parts.Length < 3) // min format is symbol_origin_ticks
                return false;

            int i = parts.Length - 1;

            var hasSide = TryParseStrToEnum(parts[i], out Feed.Types.MarketSide side); // tick doesn't have a side

            if (hasSide) // jump to expected timeframe
                i--;

            if (!TryParseStrToEnum(parts[i], out Feed.Types.Timeframe timeframe) || (timeframe.IsTick() && hasSide))
                return false;

            if (!TryParseStrToEnum(parts[--i], out SymbolConfig.Types.SymbolOrigin origin))
                return false;

            var symbol = string.Join(Separator, parts[..i]);

            if (symbol.Length == 0)
                return false;

            key = new FeedCacheKey(symbol, timeframe, origin, side);

            return true;
        }

        private static bool TryParseStrToEnum<TEnum>(string str, out TEnum value) where TEnum : struct
        {
            value = default;

            var names = Enum.GetNames(typeof(TEnum)).Select(u => u.ToLowerInvariant()).ToList();
            var index = names.IndexOf(str.ToLowerInvariant());

            if (index != -1)
                value = (TEnum)Enum.ToObject(typeof(TEnum), index);

            return index != -1;
        }

        public override bool Equals(object obj)
        {
            return obj is FeedCacheKey other &&
                   string.Equals(other.Symbol, Symbol, StringComparison.OrdinalIgnoreCase) &&
                   other.Origin == Origin &&
                   other.MarketSide == MarketSide &&
                   other.TimeFrame == TimeFrame;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Symbol.ToLowerInvariant().GetHashCode();
                hash = (hash * 16777619) ^ MarketSide.GetHashCode();
                hash = (hash * 16777619) ^ TimeFrame.GetHashCode();
                hash = (hash * 16777619) ^ (Origin + 1).GetHashCode();
                return hash;
            }
        }
    }
}
