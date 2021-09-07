using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public static class BarExtentions
    {
        private static readonly Dictionary<Feed.Types.Timeframe, int> _frameWeights = new Dictionary<Feed.Types.Timeframe, int>();

        static BarExtentions()
        {
            _frameWeights.Add(Feed.Types.Timeframe.S1, 1);
            _frameWeights.Add(Feed.Types.Timeframe.S10, 10);
            _frameWeights.Add(Feed.Types.Timeframe.M1, 60);
            _frameWeights.Add(Feed.Types.Timeframe.M5, 60 * 5);
            _frameWeights.Add(Feed.Types.Timeframe.M15, 60 * 15);
            _frameWeights.Add(Feed.Types.Timeframe.M30, 60 * 30);
            _frameWeights.Add(Feed.Types.Timeframe.H1, 60 * 60);
            _frameWeights.Add(Feed.Types.Timeframe.H4, 60 * 60 * 4);
            _frameWeights.Add(Feed.Types.Timeframe.D, 60 * 60 * 24);
            _frameWeights.Add(Feed.Types.Timeframe.W, 60 * 60 * 24 * 7);
            _frameWeights.Add(Feed.Types.Timeframe.MN, 60 * 60 * 24 * 30);
        }

        public static IEnumerable<BarData> Transform(this IEnumerable<BarData> src, Feed.Types.Timeframe targetTimeframe)
        {
            var builder = BarSequenceBuilder.Create(targetTimeframe);

            foreach (var srcBar in src)
            {
                var closedBar = builder.AppendBarPart(srcBar);
                if (closedBar != null)
                    yield return closedBar;
            }

            var finalBar = builder.CloseSequence();
            if (finalBar != null)
                yield return finalBar;
        }

        public static int GetApproximateTransformSize(Feed.Types.Timeframe srcTimeFrame, int srcCount, Feed.Types.Timeframe targetTimeframe)
        {
            var srcWeight = _frameWeights[srcTimeFrame];
            var targetWeight = _frameWeights[targetTimeframe];

            if (srcWeight >= targetWeight)
                return srcCount;

            return (int)(((double)srcCount * srcWeight) / targetWeight );
        }

        public static Feed.Types.Timeframe AdjustTimeframe(Feed.Types.Timeframe currentFrame, int currentSize, int requiredSize, out int aproxNewSize)
        {
            for (var i = currentFrame; i > Feed.Types.Timeframe.MN; i--)
            {
                aproxNewSize = GetApproximateTransformSize(currentFrame, currentSize, i);
                if (aproxNewSize <= requiredSize)
                    return i;
            }

            aproxNewSize = BarExtentions.GetApproximateTransformSize(currentFrame, currentSize, Feed.Types.Timeframe.MN);
            return Feed.Types.Timeframe.MN;
        }
    }
}
