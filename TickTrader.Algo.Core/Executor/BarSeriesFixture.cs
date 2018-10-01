using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class BarSeriesFixture : FeedFixture, IFeedBuffer
    {
        private BarSampler sampler;
        private List<BarEntity> futureBarCache;
        private ITimeRef refTimeline;
        private BarPriceType priceType;
        private double defaultBarValue;

        public BarSeriesFixture(string symbolCode, IFixtureContext context, List<BarEntity> data = null, ITimeRef refTimeline = null)
            : this(symbolCode, BarPriceType.Bid, context, data, refTimeline)
        {
        }

        public BarSeriesFixture(string symbolCode, BarPriceType priceType, IFixtureContext context, List<BarEntity> data = null, ITimeRef refTimeline = null)
            : base(symbolCode, context)
        {
            this.refTimeline = refTimeline;
            this.priceType = priceType;

            sampler = BarSampler.Get(context.TimeFrame);

            if (refTimeline != null)
            {
                futureBarCache = new List<BarEntity>();
                refTimeline.Appended += RefTimeline_Appended;
            }

            var key = BarStrategy.GetKey(SymbolCode, priceType);
            Buffer = context.Builder.GetBarBuffer(key);

            if (data != null)
                AppendData(data);
        }

        internal InputBuffer<BarEntity> Buffer { get; private set; }
        public int Count { get { return Buffer.Count; } }
        public int LastIndex { get { return Buffer.Count - 1; } }
        public DateTime this[int index] { get { return Buffer[index].OpenTime; } }
        public bool IsLoaded { get; private set; }
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

        public BufferUpdateResult Update(RateUpdate update)
        {
            if (update is QuoteEntity)
                return Update((Quote)update);
            else if (update is BarRateUpdate)
                return Update((BarRateUpdate)update);
            else
                throw new Exception("Unsupported RateUpdate implementation!");
        }

        public BufferUpdateResult Update(Quote quote)
        {
            var barBoundaries = sampler.GetBar(quote.Time);
            var barOpenTime = barBoundaries.Open;
            var price = priceType == BarPriceType.Ask ? quote.Ask : quote.Bid;

            // validate against time boundaries
            if (!Context.BufferingStrategy.InBoundaries(barOpenTime))
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

        public BufferUpdateResult Update(BarRateUpdate update)
        {
            if (priceType == BarPriceType.Bid)
                return Update(update.BidBar);
            else
                return Update(update.AskBar);
        }

        public BufferUpdateResult Update(BarEntity bar)
        {
            var barOpenTime = sampler.GetBar(bar.OpenTime).Open;

            if (!Context.BufferingStrategy.InBoundaries(barOpenTime))
                return new BufferUpdateResult();

            if (Count > 0)
            {
                var lastBar = LastBar;

                // validate agains last bar
                if (barOpenTime < lastBar.OpenTime)
                    return new BufferUpdateResult();
                else if (barOpenTime == lastBar.OpenTime)
                {
                    lastBar.Append(bar);
                    return new BufferUpdateResult() { IsLastUpdated = true };
                }
            }

            AppendBar(bar);
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
                    return;
                }
                else if (futureBarCache[0].OpenTime < timeCoordinate)
                    futureBarCache.RemoveAt(0);
                else if (futureBarCache[0].OpenTime > timeCoordinate)
                    break;
            }

            Buffer.Append(CreateFillingBar(timeCoordinate));
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

                    Buffer.Append(CreateFillingBar(timeCoordinate)); // fill empty spaces
                }

                futureBarCache.Add(bar); // place not found - add to future cache
            }
            else
            {
                Buffer.Append(bar);
                Appended?.Invoke();
            }
        }

        private BarEntity CreateFillingBar(DateTime openTime)
        {
            return CreateFillingBar(openTime, Buffer.Count > 0 ? Buffer.Last.Close : defaultBarValue);
        }

        private BarEntity CreateFillingBar(DateTime openTime, double price)
        {
            var boundaries = sampler.GetBar(openTime);
            return new BarEntity(boundaries.Open, boundaries.Close, price, double.IsNaN(price) ? price : 0);
        }

        private void AppendData(List<BarEntity> data)
        {
            IsLoaded = true;

            defaultBarValue = double.NaN;
            if (data != null)
            {
                if (data.Count > 0)
                    defaultBarValue = data[0].Open;
                data.ForEach(AppendBar);
            }

            if (refTimeline != null)
            {
                // fill end of buffer
                for (int i = Buffer.Count; i <= refTimeline.LastIndex; i++)
                    Buffer.Append(CreateFillingBar(refTimeline[i]));
            }
        }

        public void LoadFeed(DateTime from, DateTime to)
        {
            var data = Context.FeedProvider.QueryBars(SymbolCode, priceType, from, to, Context.TimeFrame);
            AppendData(data);
        }

        public void LoadFeed(int size)
        {
            var to = DateTime.Now + TimeSpan.FromDays(1);
            var data = Context.FeedProvider.QueryBars(SymbolCode, priceType, to, -size, Context.TimeFrame);
            AppendData(data);
        }

        public void LoadFeed(DateTime from, int size)
        {
            var data = Context.FeedProvider.QueryBars(SymbolCode, priceType, from, size, Context.TimeFrame);
            AppendData(data);
        }
    }
}
