using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class FeedSeriesEmulator
    {
        protected readonly Dictionary<TimeFrames, BarVector> _bidBars = new Dictionary<TimeFrames, BarVector>();
        protected readonly Dictionary<TimeFrames, BarVector> _askBars = new Dictionary<TimeFrames, BarVector>();

        public IRateInfo Current { get; private set; }

        public event Action<IRateInfo> RateUpdated;

        public IEnumerable<BarEntity> QueryBars(TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            if (from > to)
                Ref.Swap(ref from, ref to);

            var vector = GetOrAddBuilder(priceType, timeFrame);
            var index = vector.BinarySearchBy(b => b.OpenTime, from, BinarySearchTypes.NearestHigher);

            if (index < 0)
                yield break;

            for (int i = index; i < vector.Count; i++)
            {
                var bar = vector[i];
                if (bar.OpenTime > to)
                    yield break;

                yield return bar;
            }
        }

        public IEnumerable<BarEntity> QueryBars(TimeFrames timeFrame, BarPriceType priceType, DateTime from, int size)
        {
            var vector = GetOrAddBuilder(priceType, timeFrame);

            if (size == 0)
                yield break;
            else if (size > 0)
            {
                var index = vector.BinarySearchBy(b => b.OpenTime, from, BinarySearchTypes.NearestHigher);

                if (index < 0)
                    yield break;

                int resultSize = Math.Min(vector.Count - index, size);

                for (int i = index; i < index + resultSize; i++)
                    yield return vector[i];
            }
            else
            {
                var index = vector.BinarySearchBy(b => b.OpenTime, from, BinarySearchTypes.NearestLower);

                if (index < 0)
                    yield break;

                int resultSize = Math.Min(index + 1, -size);

                for (int i = index - resultSize + 1; i <= index; i++)
                    yield return vector[i];
            }
        }

        public List<Domain.QuoteInfo> QueryTicks(DateTime from, DateTime to, bool level2)
        {
            return new List<Domain.QuoteInfo>();
        }

        public List<Domain.QuoteInfo> QueryTicks(DateTime from, int count, bool level2)
        {
            return new List<Domain.QuoteInfo>();
        }

        public BarVector InitSeries(TimeFrames timeframe, BarPriceType price)
        {
            return GetOrAddBuilder(price, timeframe);
        }

        public IReadOnlyList<BarEntity> GetSeriesData(TimeFrames timeframe, BarPriceType price)
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

        protected BarVector GetOrAddBuilder(BarPriceType priceType, TimeFrames timeframe)
        {
            // TO DO : build-up series data basing on data from other time frames

            if (priceType == BarPriceType.Bid)
                return GetOrAddBuilder(_bidBars, timeframe);
            else
                return GetOrAddBuilder(_askBars, timeframe);
        }

        private BarVector GetOrAddBuilder(Dictionary<TimeFrames, BarVector> collection, TimeFrames timeframe)
        {
            BarVector builder;
            if (!collection.TryGetValue(timeframe, out builder))
            {
                builder = new BarVector(timeframe);
                collection.Add(timeframe, builder);
            }
            return builder;
        }

        private void UpdateBars(BarRateUpdate barUpdate)
        {
            //if (barUpdate.BidBar.Volume != 0) // skip filler
                UpdateBars(_bidBars.Values, barUpdate.BidBar);
            //if (barUpdate.AskBar.Volume != 0) // skip filler
                UpdateBars(_askBars.Values, barUpdate.AskBar);
        }

        private void UpdateBars(IEnumerable<BarVector> collection, BarEntity bar)
        {
            foreach (var rec in collection)
                rec.AppendBarPart(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
        }

        private void UpdateBars(IRateInfo quote)
        {
            if (quote.HasBid)
            {
                foreach (var rec in _bidBars.Values)
                    rec.AppendQuote(quote.Time, quote.Bid, 1);
            }

            if (quote.HasAsk)
            {
                foreach (var rec in _askBars.Values)
                    rec.AppendQuote(quote.Time, quote.Ask, 1);
            }
        }
    }
}
