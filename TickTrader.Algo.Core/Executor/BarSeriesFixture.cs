﻿using System;
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

            if (refTimeline == null)
            {
                ModelTimeline = new TimeRefFixture(context);
            }
            else
            {
                futureBarCache = new List<BarEntity>();
            }

            var key = BarStrategy.GetKey(SymbolCode, priceType);
            Buffer = context.Builder.GetBarBuffer(key);

            if (data != null)
                AppendSnapshot(data);
        }

        internal InputBuffer<BarEntity> Buffer { get; private set; }
        internal TimeRefFixture ModelTimeline { get; }
        public int Count { get { return Buffer.Count; } }
        public int LastIndex { get { return Buffer.Count - 1; } }
        public DateTime this[int index] { get { return Buffer[index].OpenTime; } }
        public bool IsLoaded { get; private set; }
        public DateTime OpenTime => Buffer[0].OpenTime;
        public event Action Appended;

        protected BarEntity LastBar { get; private set; }

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

            if (double.IsNaN(price))
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
            {
                if (update.HasBid)
                    return Update(update.BidBar);
            }
            else
            {
                if (update.HasAsk)
                    return Update(update.AskBar);
            }

            return new BufferUpdateResult();
        }

        public BufferUpdateResult Update(BarEntity bar)
        {
            if (!IsLoaded)
                return new BufferUpdateResult();

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

        public void SyncByTime()
        {
            for (int i = Buffer.Count; i <= refTimeline.LastIndex; i++)
            {
                var timeCoordinate = refTimeline[i];
                // fill empty spaces
                var fillBar = CreateFillingBar(timeCoordinate);
                Buffer.Append(fillBar);
                LastBar = fillBar;
            }

            refTimeline.Appended += RefTimeline_Appended;
        }

        private void RefTimeline_Appended()
        {
            var atIndex = refTimeline.LastIndex;
            var timeCoordinate = refTimeline[atIndex];

            while (futureBarCache.Count > 0)
            {
                if (futureBarCache[0].OpenTime == timeCoordinate)
                {
                    AppendBarToBuffer(futureBarCache[0]);
                    return;
                }
                else if (futureBarCache[0].OpenTime < timeCoordinate)
                    futureBarCache.RemoveAt(0);
                else if (futureBarCache[0].OpenTime > timeCoordinate)
                    break;
            }

            var fillingBar = CreateFillingBar(timeCoordinate);
            AppendBarToBuffer(fillingBar);
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
                        AppendBarToBuffer(bar);
                        return;
                    }
                    else if (timeCoordinate > bar.OpenTime) // place not found - throw out
                        return;

                    // fill empty spaces
                    var fillBar = CreateFillingBar(timeCoordinate);
                    AppendBarToBuffer(fillBar);
                }

                futureBarCache.Add(bar); // place not found - add to future cache
                LastBar = bar;
            }
            else
                AppendBarToBuffer(bar);
        }

        private void AppendBarToBuffer(BarEntity bar)
        {
            Buffer.Append(bar);
            LastBar = bar;
            Appended?.Invoke();
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

        private void AppendSnapshot(List<BarEntity> data)
        {
            IsLoaded = true;

            ModelTimeline?.InitTimeline(data);

            defaultBarValue = 0;
            if (data != null)
            {
                if (data.Count > 0)
                    defaultBarValue = data[0].Open;
                foreach (var bar in data)
                    AppendBar(bar);
            }

            if (refTimeline != null)
            {
                // fill end of buffer
                for (int i = Buffer.Count; i <= refTimeline.LastIndex; i++)
                {
                    var fillBar = CreateFillingBar(refTimeline[i]);
                    Buffer.Append(fillBar);
                    LastBar = fillBar;
                }
            }
        }

        public void LoadFeed(DateTime from, DateTime to)
        {
            var data = Context.FeedHistory.QueryBars(SymbolCode, priceType, from, to, Context.TimeFrame);
            AppendSnapshot(data);
        }

        public void LoadFeed(int size)
        {
            var to = DateTime.Now + TimeSpan.FromDays(1);
            var data = Context.FeedHistory.QueryBars(SymbolCode, priceType, to, -size, Context.TimeFrame);
            AppendSnapshot(data);
        }

        public void LoadFeed(DateTime from, int size)
        {
            var data = Context.FeedHistory.QueryBars(SymbolCode, priceType, from, size, Context.TimeFrame);
            AppendSnapshot(data);
        }

        public void LoadFeedFrom(DateTime from)
        {
            var to = DateTime.UtcNow + TimeSpan.FromDays(2);
            var data = Context.FeedHistory.QueryBars(SymbolCode, priceType, from, to, Context.TimeFrame);
            AppendSnapshot(data);
        }
    }
}
