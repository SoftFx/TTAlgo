﻿using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class BarSeriesFixture : FeedFixture, IFeedBuffer
    {
        private BarSampler sampler;
        private List<BarData> _futureBarCache;
        private ITimeRef _refTimeline;
        private Feed.Types.MarketSide _marketSide;
        private double _defaultBarValue;

        public BarSeriesFixture(string symbolCode, IFixtureContext context, List<BarData> data = null, ITimeRef refTimeline = null)
            : this(symbolCode, Feed.Types.MarketSide.Bid, context, data, refTimeline)
        {
        }

        public BarSeriesFixture(string symbolCode, Feed.Types.MarketSide marketSide, IFixtureContext context, List<BarData> data = null, ITimeRef refTimeline = null)
            : base(symbolCode, context)
        {
            _refTimeline = refTimeline;
            _marketSide = marketSide;

            sampler = BarSampler.Get(context.TimeFrame);

            if (refTimeline != null)
                _futureBarCache = new List<BarData>();

            var key = BarStrategy.GetKey(SymbolCode, marketSide);
            Buffer = context.Builder.GetBarBuffer(key);

            if (data != null)
                AppendSnapshot(data);
        }

        internal InputBuffer<BarData> Buffer { get; private set; }
        public int Count { get { return Buffer.Count; } }
        public int LastIndex { get { return Buffer.Count - 1; } }
        public UtcTicks this[int index] { get { return Buffer[index].OpenTime; } }
        public bool IsLoaded { get; private set; }
        public UtcTicks OpenTime => Buffer[0].OpenTime;

        public event Action Appended;

        protected BarData LastBar { get; private set; }

        public BufferUpdateResult Update(IRateInfo update)
        {
            if (update is QuoteInfo)
                return Update((QuoteInfo)update);
            else if (update is BarRateUpdate)
                return Update((BarRateUpdate)update);
            else
                throw new Exception("Unsupported RateUpdate implementation!");
        }

        public BufferUpdateResult Update(QuoteInfo quote)
        {
            var barBoundaries = sampler.GetBar(quote.Time);
            var barOpenTime = barBoundaries.Open;
            var price = _marketSide == Feed.Types.MarketSide.Ask ? quote.Ask : quote.Bid;

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

            var newBar = new BarData(barOpenTime, barBoundaries.Close, price, 1);
            AppendBar(newBar);
            return new BufferUpdateResult() { ExtendedBy = 1 };
        }

        public BufferUpdateResult Update(BarRateUpdate update)
        {
            if (_marketSide == Feed.Types.MarketSide.Bid)
            {
                if (update.HasBid)
                    return Update(update.BidBar, update.IsPartialUpdate);
            }
            else
            {
                if (update.HasAsk)
                    return Update(update.AskBar, update.IsPartialUpdate);
            }

            return new BufferUpdateResult();
        }

        public BufferUpdateResult Update(BarData bar, bool isPartialUpdate)
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
                    if (isPartialUpdate)
                        lastBar.AppendPart(bar);
                    else
                        lastBar.Replace(bar);

                    return new BufferUpdateResult() { IsLastUpdated = true };
                }
            }

            AppendBar(bar);
            return new BufferUpdateResult() { ExtendedBy = 1 };
        }

        public void SyncByTime()
        {
            for (int i = Buffer.Count; i <= _refTimeline.LastIndex; i++)
            {
                var timeCoordinate = _refTimeline[i];
                // fill empty spaces
                var fillBar = CreateFillingBar(timeCoordinate);
                Buffer.Append(fillBar);
                LastBar = fillBar;
            }

            _refTimeline.Appended += RefTimeline_Appended;
        }

        private void RefTimeline_Appended()
        {
            var atIndex = _refTimeline.LastIndex;
            var timeCoordinate = _refTimeline[atIndex];

            while (_futureBarCache.Count > 0)
            {
                if (_futureBarCache[0].OpenTime == timeCoordinate)
                {
                    AppendBarToBuffer(_futureBarCache[0]);
                    return;
                }
                else if (_futureBarCache[0].OpenTime < timeCoordinate)
                    _futureBarCache.RemoveAt(0);
                else if (_futureBarCache[0].OpenTime > timeCoordinate)
                    break;
            }

            var fillingBar = CreateFillingBar(timeCoordinate);
            AppendBarToBuffer(fillingBar);
        }

        private void AppendBar(BarData bar)
        {
            if (_refTimeline != null)
            {
                for (int i = Buffer.Count; i <= _refTimeline.LastIndex; i++)
                {
                    var timeCoordinate = _refTimeline[i];
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

                _futureBarCache.Add(bar); // place not found - add to future cache
                LastBar = bar;
            }
            else
                AppendBarToBuffer(bar);
        }

        private void AppendBarToBuffer(BarData bar)
        {
            Buffer.Append(bar);
            LastBar = bar;
            Appended?.Invoke();
        }

        private BarData CreateFillingBar(UtcTicks openTime)
        {
            return CreateFillingBar(openTime, Buffer.Count > 0 ? Buffer.Last.Close : _defaultBarValue);
        }

        private BarData CreateFillingBar(UtcTicks openTime, double price)
        {
            var boundaries = sampler.GetBar(openTime);
            return new BarData(boundaries.Open, boundaries.Close, price, double.IsNaN(price) ? price : 0);
        }

        private void AppendSnapshot(List<BarData> bars)
        {
            IsLoaded = true;

            _defaultBarValue = 0;
            if (bars != null)
            {
                if (bars.Count > 0)
                    _defaultBarValue = bars[0].Open;
                foreach (var bar in bars)
                    AppendBar(bar);
            }

            if (_refTimeline != null)
            {
                // fill end of buffer
                for (int i = Buffer.Count; i <= _refTimeline.LastIndex; i++)
                {
                    var fillBar = CreateFillingBar(_refTimeline[i]);
                    Buffer.Append(fillBar);
                    LastBar = fillBar;
                }
            }
        }

        public void LoadFeed(UtcTicks from, UtcTicks to)
        {
            var data = Context.FeedHistory.QueryBars(SymbolCode, _marketSide, Context.TimeFrame, from, to);
            AppendSnapshot(data);
        }

        public void LoadFeed(int size)
        {
            var to = UtcTicks.Now + TimeSpan.FromDays(1);
            var data = Context.FeedHistory.QueryBars(SymbolCode, _marketSide, Context.TimeFrame, to, -size);
            AppendSnapshot(data);
        }

        public void LoadFeed(UtcTicks from, int size)
        {
            var data = Context.FeedHistory.QueryBars(SymbolCode, _marketSide, Context.TimeFrame, from, size);
            AppendSnapshot(data);
        }

        public void LoadFeedFrom(UtcTicks from)
        {
            var data = from == null ? null
                : Context.FeedHistory.QueryBars(SymbolCode, _marketSide, Context.TimeFrame, from, UtcTicks.Now + TimeSpan.FromDays(2));
            AppendSnapshot(data);
        }
    }
}
