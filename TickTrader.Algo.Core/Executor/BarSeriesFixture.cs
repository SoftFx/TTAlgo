using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Math;

namespace TickTrader.Algo.Core
{
    internal class BarSeriesFixture : FeedFixture, ITimeRef
    {
        private BarSampler sampler;
        private InputBuffer<BarEntity> buffer;

        public BarSeriesFixture(string symbolCode, IFeedStrategyContext context)
            : base(symbolCode, context)
        {
            sampler = BarSampler.Get(context.TimeFrame);

            var data = context.QueryBars(SymbolCode, context.TimePeriodStart, context.TimePeriodEnd, context.TimeFrame);

            buffer = context.Builder.GetBarBuffer(SymbolCode);
            if (data != null)
                buffer.Append(data);
        }

        public int Depth { get { return 1; } }
        public int Count { get { return buffer.Count; } }

        public override BufferUpdateResults Update(QuoteEntity quote)
        {
            var barBoundaries = sampler.GetBar(quote.Time);
            var barOpenTime = barBoundaries.Open;

            // validate against time boundaries
            if (barOpenTime < Context.TimePeriodStart || barOpenTime >= Context.TimePeriodEnd)
                return BufferUpdateResults.NotUpdated;

            if (Count > 0)
            {
                var lastBar = buffer.Last;

                // validate agains last bar
                if (barOpenTime < lastBar.OpenTime)
                    return BufferUpdateResults.NotUpdated;
                else if (barOpenTime == lastBar.OpenTime)
                {
                    buffer.Last.Append(quote.Bid);
                    return BufferUpdateResults.LastItemUpdated;
                }
            }

            var newBar = new BarEntity(barOpenTime, barBoundaries.Close, quote);
            buffer.Append(newBar);
            return BufferUpdateResults.Extended;
        }

        public DateTime? GetTimeAtIndex(int index)
        {
            if (index < 0 || index >= Count)
                return null;
            return buffer[index].OpenTime;
        }
    }

    internal class TimeSyncBarSeriesFixture : BarSeriesFixture
    {
        public TimeSyncBarSeriesFixture(string symbolCode, IFeedStrategyContext context, ITimeRef syncRef)
            : base(symbolCode, context)
        {
        }
    }
}
