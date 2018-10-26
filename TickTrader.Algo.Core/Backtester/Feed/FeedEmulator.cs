using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class FeedEmulator : CrossDomainObject, IPluginFeedProvider, ISynchronizationContext
    {
        private Dictionary<string, FeedSeriesEmulator> _feedSources = new Dictionary<string, FeedSeriesEmulator>();

        ISynchronizationContext IPluginFeedProvider.Sync => this;

        internal IEnumerable<RateUpdate> GetFeedStream()
        {
            return GetJoinedStream();
        }

        public override void Dispose()
        {
            foreach (var src in _feedSources.Values)
                src.Stop();

            base.Dispose();
        }

        private IEnumerable<RateUpdate> GetJoinedStream()
        {
            var streams = _feedSources.Values.ToList();

            foreach (var e in streams.ToList())
            {
                e.Start();

                if (!e.MoveNext())
                {
                    streams.Remove(e);
                    e.Stop();
                }
                else
                    yield return e.Current;
            }

            while (streams.Count > 0)
            {
                var max = streams.MaxBy(e => e.Current.Time);
                var nextQuote = max.Current;

                if (!max.MoveNext())
                {
                    streams.Remove(max);
                    max.Stop();
                }

                yield return nextQuote;
            }
        }

        internal BarVector GetBarBuilder(string symbol, TimeFrames timeframe, BarPriceType price)
        {
            var stream = GetFeedSrcOrNull(symbol) ?? throw new InvalidOperationException("No feed source for symbol " + symbol);
            return stream.InitSeries(timeframe, price);
        }

        public void AddBarBuilder(string symbol, TimeFrames timeframe, BarPriceType price)
        {
            var stream = GetFeedSrcOrNull(symbol) ?? throw new InvalidOperationException("No feed source for symbol " + symbol);
            stream.InitSeries(timeframe, price);
        }

        public IReadOnlyList<BarEntity> GetBarSeriesData(string symbol, TimeFrames timeframe, BarPriceType price)
        {
            var stream = GetFeedSrcOrNull(symbol) ?? throw new InvalidOperationException("No feed source for symbol " + symbol);
            return stream.GetSeriesData(timeframe, price);
        }

        public void AddSource(string symbol, IEnumerable<QuoteEntity> stream)
        {            
            _feedSources.Add(symbol, new TickBasedSeriesEmulator(symbol, stream));
        }

        public void AddSource(string symbol, ITickStorage storage)
        {
            //if (timeFrame != TimeFrames.Ticks && timeFrame != TimeFrames.TicksLevel2)
            //    throw new ArgumentException("timeFrame", "This overload accept only TimeFrames.Ticks or TimeFrames.TicksLevel2.");

            _feedSources.Add(symbol, new TickBasedSeriesEmulator(symbol, storage));
        }

        public void AddSource(string symbol, TimeFrames timeFrame, IEnumerable<BarEntity> bidStream, IEnumerable<BarEntity> askStream)
        {
            _feedSources.Add(symbol, new BarBasedSeriesEmulator(symbol, timeFrame, bidStream, askStream));
        }

        public void AddSource(string symbol, TimeFrames timeFrame, IBarStorage bidStream, IBarStorage askStream)
        {
            _feedSources.Add(symbol, new BarBasedSeriesEmulator(symbol, timeFrame, bidStream, askStream));
        }

        private FeedSeriesEmulator GetFeedSrcOrNull(string symbol)
        {
            FeedSeriesEmulator src;
            _feedSources.TryGetValue(symbol, out src);
            return src;
        }

        #region IPluginFeedProvider

        IEnumerable<QuoteEntity> IPluginFeedProvider.GetSnapshot()
        {
            return _feedSources.Values.Select(s => (QuoteEntity)s.Current.LastQuote).ToList();
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return GetFeedSrcOrNull(symbolCode).QueryBars(timeFrame, priceType, from, to).ToList() ?? new List<BarEntity>();
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, int size, TimeFrames timeFrame)
        {
            return GetFeedSrcOrNull(symbolCode).QueryBars(timeFrame, priceType, from, size).ToList() ?? new List<BarEntity>();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, DateTime to, bool level2)
        {
            return GetFeedSrcOrNull(symbolCode).QueryTicks(from, to, level2) ?? new List<QuoteEntity>();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, int count, bool level2)
        {
            return GetFeedSrcOrNull(symbolCode).QueryTicks(from, count, level2) ?? new List<QuoteEntity>();
        }

        void IPluginFeedProvider.Subscribe(Action<QuoteEntity[]> FeedUpdated)
        {
        }

        void IPluginFeedProvider.Unsubscribe()
        {
        }

        void IPluginFeedProvider.SetSymbolDepth(string symbolCode, int depth)
        {
        }

        #endregion

        public void Invoke(Action action)
        {
            action();
        }
    }
}
