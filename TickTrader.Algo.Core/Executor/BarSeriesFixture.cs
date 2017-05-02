using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Math;

namespace TickTrader.Algo.Core
{
    internal class BarSeriesFixture : FeedFixture, ITimeRef
    {
        private BarSampler sampler;
        private List<BarEntity> futureBarCache;
        private ITimeRef refTimeline;
        private BarPriceType priceType;

        public BarSeriesFixture(string symbolCode, IFeedFixtureContext context, List<BarEntity> data = null, ITimeRef refTimeline = null)
            : this(symbolCode, BarPriceType.Bid, context, data, refTimeline)
        {
        }

        public BarSeriesFixture(string symbolCode, BarPriceType priceType, IFeedFixtureContext context, List<BarEntity> data = null, ITimeRef refTimeline = null)
            : base(symbolCode, context)
        {
            this.refTimeline = refTimeline;
            this.priceType = priceType;

            var execContext = context.ExecContext;

            sampler = BarSampler.Get(execContext.TimeFrame);

            if (data == null)
                data = context.Feed.QueryBars(SymbolCode, priceType, execContext.TimePeriodStart, execContext.TimePeriodEnd, execContext.TimeFrame);

            if (refTimeline != null)
            {
                futureBarCache = new List<BarEntity>();
                refTimeline.Appended += RefTimeline_Appended;
            }

            var key = BarStrategy.GetKey(SymbolCode, priceType);
            Buffer = execContext.Builder.GetBarBuffer(key);
            if (data != null)
                data.ForEach(AppendBar);

            if (refTimeline != null)
            {
                // fill end of buffer
                for (int i = Buffer.Count; i <= refTimeline.LastIndex; i++)
                    Buffer.Append(CreateEmptyBar(refTimeline[i]));
            }
        }

        internal InputBuffer<BarEntity> Buffer { get; private set; }
        public int Count { get { return Buffer.Count; } }
        public int LastIndex { get { return Buffer.Count - 1; } }
        public DateTime this[int index] { get { return Buffer[index].OpenTime; } }
        public event Action Appended;

        protected BarEntity LastBar
        {
            get
            {
                if (futureBarCache != null && futureBarCache.Count > 0)
                    return futureBarCache.Last();
                else
                    return Buffer.Last;
            }
        }

        public BufferUpdateResult Update(Quote quote)
        {
            var barBoundaries = sampler.GetBar(quote.Time);
            var barOpenTime = barBoundaries.Open;
            var price = priceType == BarPriceType.Ask ? quote.Ask : quote.Bid;

            // validate against time boundaries
            if (barOpenTime < Context.ExecContext.TimePeriodStart || barOpenTime >= Context.ExecContext.TimePeriodEnd)
                return new BufferUpdateResult();

            if (Count > 0)
            {
                var lastBar = LastBar;

                // validate agains last bar
                if (barOpenTime < lastBar.OpenTime)
                    return new BufferUpdateResult();
                else if (barOpenTime == lastBar.OpenTime)
                {
                    lastBar.Append(price, 1);
                    return new BufferUpdateResult() { IsLastUpdated = true };
                }
            }

            var newBar = new BarEntity(barOpenTime, barBoundaries.Close, price, 1);
            AppendBar(newBar);
            return new BufferUpdateResult() { ExtendedBy = 1 };
        }

        private void RefTimeline_Appended()
        {
            var atIndex = refTimeline.LastIndex;
            var timeCoordinate = refTimeline[atIndex];

            while (futureBarCache.Count > 0)
            {
                if (futureBarCache[0].OpenTime == timeCoordinate)
                {
                    Buffer.Append(futureBarCache[0]);
                    break;
                }
                else if (futureBarCache[0].OpenTime < timeCoordinate)
                    futureBarCache.RemoveAt(0);
                else if (futureBarCache[0].OpenTime > timeCoordinate)
                    break;
            }
        }

        private void AppendBar(BarEntity bar)
        {
            if (refTimeline != null)
            {
                for (int i = Buffer.Count; i <= refTimeline.LastIndex; i++)
                {
                    var timeCoordinate = refTimeline[i];
                    if (timeCoordinate == bar.OpenTime) // found right place
                    {
                        Buffer.Append(bar);
                        return;
                    }
                    else if (timeCoordinate > bar.OpenTime) // place not found - throw out
                        return;

                    Buffer.Append(CreateEmptyBar(timeCoordinate)); // fill empty spaces
                }

                futureBarCache.Add(bar); // place not found - add to future cache
            }
            else
            {
                Buffer.Append(bar);
                Appended?.Invoke();
            }
        }

        private BarEntity CreateEmptyBar(DateTime openTime)
        {
            var boundaries = sampler.GetBar(openTime);
            return new BarEntity(boundaries.Open, boundaries.Close, double.NaN, double.NaN);
        }
    }

    //internal class TimeSyncBarSeriesFixture : BarSeriesFixture
    //{
    //    public TimeSyncBarSeriesFixture(string symbolCode, IFeedFixtureContext context, ITimeRef syncRef)
    //        : base(symbolCode, context)
    //    {
    //    }
    //}
}
