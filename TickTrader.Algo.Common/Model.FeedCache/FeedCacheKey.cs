using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model
{
    public class FeedCacheKey
    {
        public string Symbol { get; private set; }
        public TimeFrames Frame { get; private set; }
        public BarPriceType? PriceType { get; private set; }

        public FeedCacheKey(string symbol, TimeFrames timeFrame, BarPriceType? priceType = null)
        {
            Symbol = symbol;
            Frame = timeFrame;
            PriceType = priceType;
        }

        internal string Serialize()
        {
            var builder = new StringBuilder();
            builder.Append(Symbol).Append('\0');
            builder.Append(Frame);
            if (PriceType != null)
                builder.Append('\0').Append(PriceType.Value);
            return builder.ToString();
        }

        internal static FeedCacheKey Deserialize(string str)
        {
            var parts = str.Split('\0');
            if (parts.Length < 2 || parts.Length > 3)
                throw new Exception("Cannot deserialize cache key: " + str);
            var symbol = parts[0];
            var frame = (TimeFrames)Enum.Parse(typeof(TimeFrames), parts[1]);
            BarPriceType? priceType = null;
            if (parts.Length > 2)
                priceType = (BarPriceType)Enum.Parse(typeof(BarPriceType), parts[2]);
            return new FeedCacheKey(symbol, frame, priceType);
        }

        public override bool Equals(object obj)
        {
            var other = obj as FeedCacheKey;
            return other != null && other.Symbol == Symbol && other.PriceType == PriceType && other.Frame == Frame;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Symbol.GetHashCode();
                hash = (hash * 16777619) ^ PriceType.GetHashCode();
                hash = (hash * 16777619) ^ Frame.GetHashCode();
                return hash;
            }
        }
    }
}
