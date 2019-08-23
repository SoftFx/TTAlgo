using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public static class BarExtentions
    {
        private static readonly Dictionary<TimeFrames, int> _frameWeights = new Dictionary<TimeFrames, int>();

        static BarExtentions()
        {
            _frameWeights.Add(TimeFrames.S1, 1);
            _frameWeights.Add(TimeFrames.S10, 10);
            _frameWeights.Add(TimeFrames.M1, 60);
            _frameWeights.Add(TimeFrames.M5, 60 * 5);
            _frameWeights.Add(TimeFrames.M15, 60 * 15);
            _frameWeights.Add(TimeFrames.M30, 60 * 30);
            _frameWeights.Add(TimeFrames.H1, 60 * 60);
            _frameWeights.Add(TimeFrames.H4, 60 * 60 * 4);
            _frameWeights.Add(TimeFrames.D, 60 * 60 * 24);
            _frameWeights.Add(TimeFrames.W, 60 * 60 * 24 * 7);
            _frameWeights.Add(TimeFrames.MN, 60 * 60 * 24 * 30);
        }

        public static IEnumerable<BarEntity> Transform(this IEnumerable<BarEntity> src, TimeFrames targetTimeframe)
        {
            var builder = BarSequenceBuilder.Create(targetTimeframe);

            foreach (var srcBar in src)
            {
                var closedBar = builder.AppendBarPart(srcBar.OpenTime, srcBar.Open, srcBar.High, srcBar.Low, srcBar.Close, srcBar.Volume);
                if (closedBar != null)
                    yield return closedBar;
            }

            var finalBar = builder.CloseSequence();
            if (finalBar != null)
                yield return finalBar;
        }

        public static int GetApproximateTransformSize(TimeFrames srcTimeFrame, int srcCount, TimeFrames targetTimeframe)
        {
            var srcWeight = _frameWeights[srcTimeFrame];
            var targetWeight = _frameWeights[targetTimeframe];

            if (srcWeight >= targetWeight)
                return srcCount;

            return (int)(((double)srcCount * srcWeight) / targetWeight );
        }

        public static TimeFrames AdjustTimeframe(TimeFrames currentFrame, int currentSize, int requiredSize, out int aproxNewSize)
        {
            for (var i = currentFrame; i > TimeFrames.MN; i--)
            {
                aproxNewSize = GetApproximateTransformSize(currentFrame, currentSize, i);
                if (aproxNewSize <= requiredSize)
                    return i;
            }

            aproxNewSize = BarExtentions.GetApproximateTransformSize(currentFrame, currentSize, TimeFrames.MN);
            return TimeFrames.MN;
        }
    }
}
