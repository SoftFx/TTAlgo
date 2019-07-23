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
        //private FeedReader _reader;
        private Dictionary<string, SeriesReader> _feedReaders = new Dictionary<string, SeriesReader>();
        private Dictionary<string, FeedSeriesEmulator> _feedSeries = new Dictionary<string, FeedSeriesEmulator>();

        ISynchronizationContext IPluginFeedProvider.Sync => this;

        public FeedEmulator()
        {
        }

        private FeedEmulator(FeedEmulator src)
        {
            foreach (var rec in src._feedReaders)
            {
                _feedReaders.Add(rec.Key, rec.Value.Clone());
                _feedSeries.Add(rec.Key, new FeedSeriesEmulator());
            }
        }

        internal IEnumerable<RateUpdate> GetFeedStream()
        {
            var reader = new FeedReader(_feedReaders.Values);

            using (reader)
            {
                foreach (var rate in reader)
                {
                    FeedSeriesEmulator series;
                    if (_feedSeries.TryGetValue(rate.Symbol, out series))
                        series.Update(rate);
                    yield return rate;
                }
            }

            foreach (var series in _feedSeries.Values)
                series.Close();

            if (reader.HasFailed)
                throw new Exception("Failed to read feed stream! " + reader.Fault.Message, reader.Fault);
        }

        internal BarVector GetBarBuilder(string symbol, TimeFrames timeframe, BarPriceType price)
        {
            return GetFeedSrcOrThrow(symbol).InitSeries(timeframe, price);
        }

        internal FeedSeriesEmulator GetFeedSymbolFixture(string symbol, TimeFrames timeframe)
        {
            return GetFeedSrcOrThrow(symbol);
        }

        public void AddBarBuilder(string symbol, TimeFrames timeframe, BarPriceType price)
        {
            GetFeedSrcOrThrow(symbol).InitSeries(timeframe, price);
        }

        public IReadOnlyList<BarEntity> GetBarSeriesData(string symbol, TimeFrames timeframe, BarPriceType price)
        {
            return GetFeedSrcOrThrow(symbol).GetSeriesData(timeframe, price);
        }

        public void AddSource(string symbol, IEnumerable<QuoteEntity> stream)
        {
            _feedReaders.Add(symbol, new TickSeriesReader(symbol, stream));
            _feedSeries.Add(symbol, new FeedSeriesEmulator());
        }

        public void AddSource(string symbol, ITickStorage storage)
        {
            //if (timeFrame != TimeFrames.Ticks && timeFrame != TimeFrames.TicksLevel2)
            //    throw new ArgumentException("timeFrame", "This overload accept only TimeFrames.Ticks or TimeFrames.TicksLevel2.");

            _feedReaders.Add(symbol, new TickSeriesReader(symbol, storage));
            _feedSeries.Add(symbol, new FeedSeriesEmulator());
        }

        public void AddSource(string symbol, TimeFrames timeFrame, IEnumerable<BarEntity> bidStream, IEnumerable<BarEntity> askStream)
        {
            _feedReaders.Add(symbol, new BarSeriesReader(symbol, timeFrame, bidStream, askStream));
            _feedSeries.Add(symbol, new FeedSeriesEmulator());
        }

        public void AddSource(string symbol, TimeFrames timeFrame, IBarStorage bidStream, IBarStorage askStream)
        {
            _feedReaders.Add(symbol, new BarSeriesReader(symbol, timeFrame, bidStream, askStream));
            _feedSeries.Add(symbol, new FeedSeriesEmulator());
        }

        private FeedSeriesEmulator GetFeedSrcOrThrow(string symbol)
        {
            FeedSeriesEmulator src;
            if (!_feedSeries.TryGetValue(symbol, out src))
                throw new MisconfigException("No feed source for symbol " + symbol);
            return src;
        }

        internal FeedEmulator Clone()
        {
            return new FeedEmulator(this);
        }

        public override void Dispose()
        {
            foreach (var reader in _feedReaders.Values)
                reader.Stop();

            base.Dispose();
        }

        #region IPluginFeedProvider

        IEnumerable<QuoteEntity> IPluginFeedProvider.GetSnapshot()
        {
            return _feedSeries.Values.Where(s => s.Current != null).Select(s => (QuoteEntity)s.Current.LastQuote).ToList();
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return GetFeedSrcOrThrow(symbolCode).QueryBars(timeFrame, priceType, from, to).ToList() ?? new List<BarEntity>();
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, int size, TimeFrames timeFrame)
        {
            return GetFeedSrcOrThrow(symbolCode).QueryBars(timeFrame, priceType, from, size).ToList() ?? new List<BarEntity>();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, DateTime to, bool level2)
        {
            return GetFeedSrcOrThrow(symbolCode).QueryTicks(from, to, level2) ?? new List<QuoteEntity>();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, int count, bool level2)
        {
            return GetFeedSrcOrThrow(symbolCode).QueryTicks(from, count, level2) ?? new List<QuoteEntity>();
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

        public void Send(Action action)
        {
            action();
        }
    }
}
