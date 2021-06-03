using System;
using System.Text;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage
{

    public class FeedCacheKey
    {
        public string Symbol { get; private set; }
        public Feed.Types.Timeframe Frame { get; private set; }
        public Feed.Types.MarketSide? MarketSide { get; private set; }

        public FeedCacheKey(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide? priceType = null)
        {
            Symbol = symbol;
            Frame = timeframe;
            if (Frame == Feed.Types.Timeframe.Ticks || Frame == Feed.Types.Timeframe.TicksLevel2)
                MarketSide = null;
            else
                MarketSide = priceType;
        }

        internal string ToCodeString()
        {
            var builder = new StringBuilder();
            builder.Append(Symbol).Append('_');
            builder.Append(Frame);
            builder.Append('_');
            if (MarketSide != null)
                builder.Append(MarketSide.Value);
            return builder.ToString();
        }

        internal static bool TryParse(string strCode, out FeedCacheKey key)
        {
            key = null;
            Feed.Types.MarketSide? side;
            Feed.Types.Timeframe timeframe;
            string symbol;

            var parser = new TextParser(strCode);

            // price

            var sidePart = parser.ReadNextFromEnd('_');

            if (sidePart == null)
                return false;
            else if (sidePart == string.Empty)
                side = null;
            else
            {
                if (!Enum.TryParse(sidePart, out Feed.Types.MarketSide parsedSide))
                    return false;
                side = parsedSide;
            }

            // time frame

            var timeframePart  = parser.ReadNextFromEnd('_');

            if (string.IsNullOrEmpty(timeframePart))
                return false;

            if (!Enum.TryParse(timeframePart, out timeframe))
                return false;

            // symbol

            symbol = parser.GetRemainingText();

            if (string.IsNullOrEmpty(symbol))
                return false;

            key = new FeedCacheKey(symbol, timeframe, side);
            return true;


            //var parts = str.Split('\0');
            //if (parts.Length < 2 || parts.Length > 3)
            //throw new Exception("Cannot deserialize cache key: " + str);
            //var symbol = parts[0];
            //var frame = (Feed.Types.Timeframe)Enum.Parse(typeof(Feed.Types.Timeframe), parts[1]);
            //Feed.Types.MarketSide? priceType = null;
            //if (parts.Length > 2)
            //    priceType = (Feed.Types.MarketSide)Enum.Parse(typeof(Feed.Types.MarketSide), parts[2]);
            //return new FeedCacheKey(symbol, frame, priceType);
        }

        public override bool Equals(object obj)
        {
            var other = obj as FeedCacheKey;
            return other != null && other.Symbol == Symbol && other.MarketSide == MarketSide && other.Frame == Frame;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Symbol.GetHashCode();
                hash = (hash * 16777619) ^ MarketSide.GetHashCode();
                hash = (hash * 16777619) ^ Frame.GetHashCode();
                return hash;
            }
        }
    }
}
