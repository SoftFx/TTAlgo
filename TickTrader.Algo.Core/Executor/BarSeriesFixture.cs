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

        public BarSeriesFixture(string symbolCode, IFeedFixtureContext context, List<BarEntity> data = null)
            : base(symbolCode, context)
        {
            var execContext = context.ExecContext;

            sampler = BarSampler.Get(execContext.TimeFrame);

            if (data == null)
                data = context.Feed.QueryBars(SymbolCode, execContext.TimePeriodStart, execContext.TimePeriodEnd, execContext.TimeFrame);

            buffer = execContext.Builder.GetBarBuffer(SymbolCode);
            if (data != null)
                buffer.Append(data);
        }

        public int Count { get { return buffer.Count; } }

        public override BufferUpdateResults Update(QuoteEntity quote)
        {
            var barBoundaries = sampler.GetBar(quote.Time);
            var barOpenTime = barBoundaries.Open;

            // validate against time boundaries
            if (barOpenTime < Context.ExecContext.TimePeriodStart || barOpenTime >= Context.ExecContext.TimePeriodEnd)
                return BufferUpdateResults.NotUpdated;

            if (Count > 0)
            {
                var lastBar = buffer.Last;

                // validate agains last bar
                if (barOpenTime < lastBar.OpenTime)
                    return BufferUpdateResults.NotUpdated;
                else if (barOpenTime == lastBar.OpenTime)
                {
                    buffer.Last.Append(quote.Bid, 1);
                    return BufferUpdateResults.LastItemUpdated;
                }
            }

            var newBar = new BarEntity(barOpenTime, barBoundaries.Close, quote.Bid, 1);
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
        public TimeSyncBarSeriesFixture(string symbolCode, IFeedFixtureContext context, ITimeRef syncRef)
            : base(symbolCode, context)
        {
        }
    }
}
