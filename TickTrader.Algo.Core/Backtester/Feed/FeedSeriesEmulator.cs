using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal abstract class FeedSeriesEmulator
    {
        protected readonly Dictionary<TimeFrames, BarVector> _bidBars = new Dictionary<TimeFrames, BarVector>();
        protected readonly Dictionary<TimeFrames, BarVector> _askBars = new Dictionary<TimeFrames, BarVector>();

        public QuoteEntity Current { get; protected set; }

        public abstract void Start();
        public abstract void Stop();
        public abstract bool MoveNext();

        public IEnumerable<BarEntity> QueryBars(TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            if (from > to)
                Ref.Swap(ref from, ref to);

            var vector = GetOrAddBuilder(priceType, timeFrame);
            var index = vector.BinarySearch(b => b.OpenTime, from, BinarySearchTypes.NearestHigher);

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
                var index = vector.BinarySearch(b => b.OpenTime, from, BinarySearchTypes.NearestHigher);

                if (index < 0)
                    yield break;

                int resultSize = Math.Min(vector.Count - index, size);

                for (int i = index; i < index + resultSize; i++)
                    yield return vector[i];
            }
            else
            {
                var index = vector.BinarySearch(b => b.OpenTime, from, BinarySearchTypes.NearestLower);

                if (index < 0)
                    yield break;

                int resultSize = Math.Min(index + 1, -size);

                for (int i = index; i > index - resultSize; i--)
                    yield return vector[i];
            }
        }

        public List<QuoteEntity> QueryTicks(DateTime from, DateTime to, bool level2)
        {
            return new List<QuoteEntity>();
        }

        public List<QuoteEntity> QueryTicks(DateTime from, int count, bool level2)
        {
            return new List<QuoteEntity>();
        }

        public BarVector InitSeries(TimeFrames timeframe, BarPriceType price)
        {
            return GetOrAddBuilder(price, timeframe);
        }

        public IReadOnlyList<BarEntity> GetSeriesData(TimeFrames timeframe, BarPriceType price)
        {
            return GetOrAddBuilder(price, timeframe);
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
    }
}
