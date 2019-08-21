using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.Algo.Common.Model
{
    [Serializable]
    public class FeedCacheKey
    {
        public string Symbol { get; private set; }
        public TimeFrames Frame { get; private set; }
        public BarPriceType? PriceType { get; private set; }

        public FeedCacheKey(string symbol, TimeFrames timeFrame, BarPriceType? priceType = null)
        {
            Symbol = symbol;
            Frame = timeFrame;
            if (Frame == TimeFrames.Ticks || Frame == TimeFrames.TicksLevel2)
                PriceType = null;
            else
                PriceType = priceType;
        }

        internal string ToCodeString()
        {
            var builder = new StringBuilder();
            builder.Append(Symbol).Append('_');
            builder.Append(Frame);
            builder.Append('_');
            if (PriceType != null)
                builder.Append(PriceType.Value);
            return builder.ToString();
        }

        internal static bool TryParse(string strCode, out FeedCacheKey key)
        {
            key = null;
            BarPriceType? price;
            TimeFrames timeFrame;
            string symbol;

            var parser = new TextParser(strCode);

            // price

            var pricePart = parser.ReadNextFromEnd('_');

            if (pricePart == null)
                return false;
            else if (pricePart == string.Empty)
                price = null;
            else
            {
                BarPriceType parsedPrice;
                if (!Enum.TryParse(pricePart, out parsedPrice))
                    return false;
                price = parsedPrice;
            }

            // time frame

            var timeframePart  = parser.ReadNextFromEnd('_');

            if (string.IsNullOrEmpty(timeframePart))
                return false;

            if (!Enum.TryParse(timeframePart, out timeFrame))
                return false;

            // symbol

            symbol = parser.GetRemainingText();

            if (string.IsNullOrEmpty(symbol))
                return false;

            key = new FeedCacheKey(symbol, timeFrame, price);
            return true;


            //var parts = str.Split('\0');
            //if (parts.Length < 2 || parts.Length > 3)
            //throw new Exception("Cannot deserialize cache key: " + str);
            //var symbol = parts[0];
            //var frame = (TimeFrames)Enum.Parse(typeof(TimeFrames), parts[1]);
            //BarPriceType? priceType = null;
            //if (parts.Length > 2)
            //    priceType = (BarPriceType)Enum.Parse(typeof(BarPriceType), parts[2]);
            //return new FeedCacheKey(symbol, frame, priceType);
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
