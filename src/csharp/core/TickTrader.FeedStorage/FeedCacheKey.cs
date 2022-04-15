using System;
using System.Linq;
using System.Text;
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

            var parts = strCode.Trim(Separator).Split(Separator);

            var symbol = new StringBuilder(1 << 4);
            var origin = default(SymbolConfig.Types.SymbolOrigin);
            var timeframe = default(Feed.Types.Timeframe);
            var side = default(Feed.Types.MarketSide);

            int i = 0;

            while (i < parts.Length && !TryParseStrToEnum(parts[i], out origin)) //combline all parts before origin (ex. EURUSD_test1)
                symbol.Append(parts[i++]).Append(Separator);

            if (i == parts.Length || symbol.Length == 0)
                return false;

            var result = ++i < parts.Length && TryParseStrToEnum(parts[i], out timeframe);

            if (i + 1 < parts.Length) //getting side if it exist
                result &= TryParseStrToEnum(parts[++i], out side);

            result &= ++i == parts.Length; //check that string is over

            if (result)
                key = new FeedCacheKey(symbol.ToString().Remove(symbol.Length - 1), timeframe, origin, side);

            return result;
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
