using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    public interface ICrossDomainStorage
    {
        void Start();

        void Stop();
    }


    public interface ICrossDomainStorage<T> : ICrossDomainStorage
    {
        IEnumerable<T> GetStream();
    }


    public class FeedEmulator : IFeedProvider, IFeedHistoryProvider
    {
        private List<ICrossDomainStorage> _storages = new List<ICrossDomainStorage>();
        private Dictionary<string, SeriesReader> _feedReaders = new Dictionary<string, SeriesReader>();
        private Dictionary<string, FeedSeriesEmulator> _feedSeries = new Dictionary<string, FeedSeriesEmulator>();

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

        internal void InitStorages()
        {
            foreach (var storage in _storages)
                storage.Start();
        }

        internal void DeinitStorages()
        {
            var exceptions = new List<Exception>();

            foreach (var storage in _storages)
            {
                try
                {
                    storage.Stop();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            //if (exceptions.Count > 0)
            //    throw new AggregateException(exceptions);
        }

        internal IEnumerable<IRateInfo> GetFeedStream()
        {
            var reader = new FeedReader(_feedReaders.Values);

            using (reader)
            {
                foreach (var rate in reader)
                {
                    yield return rate;
                }
            }

            if (reader.HasFailed)
                throw new Exception("Failed to read feed stream! " + reader.Fault.Message, reader.Fault);
        }

        internal void UpdateHistory(IRateInfo rate)
        {
            FeedSeriesEmulator series;
            if (_feedSeries.TryGetValue(rate.Symbol, out series))
                series.Update(rate);
        }

        internal void CloseHistory()
        {
            foreach (var series in _feedSeries.Values)
                series.Close();
        }

        internal BarVector GetBarBuilder(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide price)
        {
            return GetFeedSrcOrThrow(symbol).InitSeries(timeframe, price);
        }

        internal FeedSeriesEmulator GetFeedSymbolFixture(string symbol, Feed.Types.Timeframe timeframe)
        {
            return GetFeedSrcOrThrow(symbol);
        }

        public void AddBarBuilder(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide price)
        {
            GetFeedSrcOrThrow(symbol).InitSeries(timeframe, price);
        }

        public IReadOnlyList<BarData> GetBarSeriesData(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide price)
        {
            return GetFeedSrcOrThrow(symbol).GetSeriesData(timeframe, price);
        }

        public void AddSource(string symbol, IEnumerable<Domain.QuoteInfo> stream)
        {
            _feedReaders.Add(symbol, new TickSeriesReader(symbol, stream));
            _feedSeries.Add(symbol, new FeedSeriesEmulator());
        }

        public void AddSource(string symbol, ICrossDomainStorage<QuoteInfo> storage)
        {
            //if (timeFrame != TimeFrames.Ticks && timeFrame != TimeFrames.TicksLevel2)
            //    throw new ArgumentException("timeFrame", "This overload accept only TimeFrames.Ticks or TimeFrames.TicksLevel2.");

            _feedReaders.Add(symbol, new TickSeriesReader(symbol, storage));
            _feedSeries.Add(symbol, new FeedSeriesEmulator());

            _storages.Add(storage);
        }

        public void AddSource(string symbol, Feed.Types.Timeframe timeframe, IEnumerable<BarData> bidStream, IEnumerable<BarData> askStream)
        {
            _feedReaders.Add(symbol, new BarSeriesReader(symbol, timeframe, bidStream, askStream));
            _feedSeries.Add(symbol, new FeedSeriesEmulator());
        }

        public void AddSource(string symbol, Feed.Types.Timeframe timeframe, ICrossDomainStorage<BarData> bidStream, ICrossDomainStorage<BarData> askStream)
        {
            _feedReaders.Add(symbol, new BarSeriesReader(symbol, timeframe, bidStream, askStream));
            _feedSeries.Add(symbol, new FeedSeriesEmulator());

            _storages.Add(bidStream);
            _storages.Add(askStream);
        }

        private FeedSeriesEmulator GetFeedSrcOrThrow(string symbol)
        {
            if (!_feedSeries.TryGetValue(symbol, out FeedSeriesEmulator src))
                throw new MisconfigException("No feed source for symbol " + symbol);
            return src;
        }

        internal FeedEmulator Clone()
        {
            return new FeedEmulator(this);
        }

        public void Dispose()
        {
            foreach (var reader in _feedReaders.Values)
                reader.Stop();
        }

        #region IPluginFeedProvider

        List<QuoteInfo> IFeedProvider.GetQuoteSnapshot()
        {
            return _feedSeries.Values.Where(s => s.Current != null).Select(s => s.Current.LastQuote).ToList();
        }

        Task<List<QuoteInfo>> IFeedProvider.GetQuoteSnapshotAsync()
        {
            return Task.FromResult(_feedSeries.Values.Where(s => s.Current != null).Select(s => s.Current.LastQuote).ToList());
        }

        IQuoteSub IFeedProvider.GetQuoteSub() => new QuoteSubStub();

        IBarSub IFeedProvider.GetBarSub() => new BarSubStub();

        private class HandlerStub : IDisposable
        {
            public void Dispose() { }
        }

        private class QuoteSubStub : IQuoteSub
        {
            public IDisposable AddHandler(Action<QuoteInfo> handler) => new HandlerStub();

            public void Dispose() { }

            public void Modify(QuoteSubUpdate update) { }

            public void Modify(List<QuoteSubUpdate> updates) { }
        }

        private class BarSubStub : IBarSub
        {
            public IDisposable AddHandler(Action<BarUpdate> handler) => new HandlerStub();

            public void Dispose() { }

            public void Modify(BarSubUpdate update) { }

            public void Modify(List<BarSubUpdate> updates) { }
        }

        List<BarData> IFeedHistoryProvider.QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, UtcTicks to)
        {
            return GetFeedSrcOrThrow(symbol).QueryBars(marketSide, timeframe, from, to).ToList() ?? new List<BarData>();
        }

        List<BarData> IFeedHistoryProvider.QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, int count)
        {
            return GetFeedSrcOrThrow(symbol).QueryBars(marketSide, timeframe, from, count).ToList() ?? new List<BarData>();
        }

        List<QuoteInfo> IFeedHistoryProvider.QueryQuotes(string symbol, UtcTicks from, UtcTicks to, bool level2)
        {
            return GetFeedSrcOrThrow(symbol).QueryTicks(from, to, level2) ?? new List<QuoteInfo>();
        }

        List<QuoteInfo> IFeedHistoryProvider.QueryQuotes(string symbol, UtcTicks from, int count, bool level2)
        {
            return GetFeedSrcOrThrow(symbol).QueryTicks(from, count, level2) ?? new List<QuoteInfo>();
        }

        Task<List<BarData>> IFeedHistoryProvider.QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, UtcTicks to)
        {
            return Task.FromResult(GetFeedSrcOrThrow(symbol).QueryBars(marketSide, timeframe, from, to).ToList() ?? new List<BarData>());
        }

        Task<List<BarData>> IFeedHistoryProvider.QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, int count)
        {
            return Task.FromResult(GetFeedSrcOrThrow(symbol).QueryBars(marketSide, timeframe, from, count).ToList() ?? new List<BarData>());
        }

        Task<List<QuoteInfo>> IFeedHistoryProvider.QueryQuotesAsync(string symbol, UtcTicks from, UtcTicks to, bool level2)
        {
            return Task.FromResult(GetFeedSrcOrThrow(symbol).QueryTicks(from, to, level2) ?? new List<QuoteInfo>());
        }

        Task<List<QuoteInfo>> IFeedHistoryProvider.QueryQuotesAsync(string symbol, UtcTicks from, int count, bool level2)
        {
            return Task.FromResult(GetFeedSrcOrThrow(symbol).QueryTicks(from, count, level2) ?? new List<QuoteInfo>());
        }

        public event Action<QuoteInfo> QuoteUpdated { add { } remove { } }
        public event Action<BarUpdate> BarUpdated { add { } remove { } }

        #endregion
    }
}
