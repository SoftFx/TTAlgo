using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal class FeedSeriesEmulator
    {
        protected readonly Dictionary<Feed.Types.Timeframe, BarVector> _bidBars = new Dictionary<Feed.Types.Timeframe, BarVector>();
        protected readonly Dictionary<Feed.Types.Timeframe, BarVector> _askBars = new Dictionary<Feed.Types.Timeframe, BarVector>();

        public IRateInfo Current { get; private set; }

        public event Action<IRateInfo> RateUpdated;

        public IEnumerable<BarData> QueryBars(Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            if (from > to)
                Ref.Swap(ref from, ref to);

            var vector = GetOrAddBuilder(marketSide, timeframe);
            var index = vector.BinarySearchBy(b => b.OpenTime, from, BinarySearchTypes.NearestHigher);

            if (index < 0)
                yield break;

            for (var i = index; i < vector.Count; i++)
            {
                var bar = vector[i];
                if (bar.OpenTime > to)
                    yield break;

                yield return bar;
            }
        }

        public IEnumerable<BarData> QueryBars(Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count)
        {
            var vector = GetOrAddBuilder(marketSide, timeframe);

            if (count == 0)
                yield break;
            else if (count > 0)
            {
                var index = vector.BinarySearchBy(b => b.OpenTime, from, BinarySearchTypes.NearestHigher);

                if (index < 0)
                    yield break;

                int resultSize = Math.Min(vector.Count - index, count);

                for (int i = index; i < index + resultSize; i++)
                    yield return vector[i];
            }
            else
            {
                var index = vector.BinarySearchBy(b => b.OpenTime, from, BinarySearchTypes.NearestLower);

                if (index < 0)
                    yield break;

                int resultSize = Math.Min(index + 1, -count);

                for (int i = index - resultSize + 1; i <= index; i++)
                    yield return vector[i];
            }
        }

        public List<QuoteInfo> QueryTicks(Timestamp from, Timestamp to, bool level2)
        {
            return new List<QuoteInfo>();
        }

        public List<QuoteInfo> QueryTicks(Timestamp from, int count, bool level2)
        {
            return new List<QuoteInfo>();
        }

        public BarVector InitSeries(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide price)
        {
            return GetOrAddBuilder(price, timeframe);
        }

        public IReadOnlyList<BarData> GetSeriesData(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide price)
        {
            return GetOrAddBuilder(price, timeframe);
        }

        public void Update(IRateInfo rate)
        {
            if (rate is BarRateUpdate)
                UpdateBars((BarRateUpdate)rate);
            else
                UpdateBars(rate);

            Current = rate;
            RateUpdated?.Invoke(rate);
        }

        public void Close()
        {
            foreach (var rec in _bidBars.Values)
                rec.Close();
        }


        protected BarVector GetOrAddBuilder(Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe)
        {
            // TO DO : build-up series data basing on data from other time frames

            if (!_bidBars.TryGetValue(timeframe, out var bidBuilder))
            {
                bidBuilder = new BarVector(timeframe);
                _bidBars.Add(timeframe, bidBuilder);
            }

            if (!_askBars.TryGetValue(timeframe, out var askBuilder))
            {
                askBuilder = new BarVector(timeframe);
                _askBars.Add(timeframe, askBuilder);
            }

            return marketSide == Feed.Types.MarketSide.Bid ? bidBuilder : askBuilder;
        }

        private void UpdateBars(BarRateUpdate barUpdate)
        {
            //if (barUpdate.BidBar.Volume != 0) // skip filler
                UpdateBars(_bidBars.Values, barUpdate.BidBar);
            //if (barUpdate.AskBar.Volume != 0) // skip filler
                UpdateBars(_askBars.Values, barUpdate.AskBar);
        }

        private void UpdateBars(IEnumerable<BarVector> collection, BarData bar)
        {
            foreach (var rec in collection)
                rec.AppendBarPart(bar);
        }

        private void UpdateBars(IRateInfo quote)
        {
            if (quote.HasBid)
            {
                foreach (var rec in _bidBars.Values)
                    rec.AppendQuote(quote.Timestamp, quote.Bid, 1);
            }

            if (quote.HasAsk)
            {
                foreach (var rec in _askBars.Values)
                    rec.AppendQuote(quote.Timestamp, quote.Ask, 1);
            }
        }
    }
}
