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

        public List<BarEntity> QueryBars(TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            return new List<BarEntity>();
        }

        public List<BarEntity> QueryBars(TimeFrames timeFrame, BarPriceType priceType, DateTime from, int size)
        {
            return new List<BarEntity>();
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

        private BarVector GetOrAddBuilder(BarPriceType priceType, TimeFrames timeframe)
        {
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
