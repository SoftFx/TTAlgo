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

        internal IPagedEnumerator<QuoteEntity> GetFeedStream()
        {
            return GetJoinedStream().GetCrossDomainEnumerator(8000);
        }

        internal bool Warmup(int quoteCount)
        {
            int i = 0;
            foreach (var q in GetJoinedStream())
            {
                if (++i >= quoteCount)
                    return true;
            }

            return false;
        }

        private IEnumerable<QuoteEntity> GetJoinedStream()
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

        internal ITimeSequenceRef GetBarBuilder(string symbol, TimeFrames timeframe, BarPriceType price)
        {
            var stream = GetFeedSrcOrNull(symbol) ?? throw new InvalidOperationException("No feed source for symbol " + symbol);
            return stream.InitSeries(timeframe, price).Ref;
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
            _feedSources.Add(symbol, new FeedSeriesEmulator.QuoteBased(stream));
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
            return new List<QuoteEntity>();
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return GetFeedSrcOrNull(symbolCode).QueryBars(timeFrame, priceType, from, to) ?? new List<BarEntity>();
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, int size, TimeFrames timeFrame)
        {
            return GetFeedSrcOrNull(symbolCode).QueryBars(timeFrame, priceType, from, size) ?? new List<BarEntity>();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, DateTime to, int depth)
        {
            return GetFeedSrcOrNull(symbolCode).QueryTicks(from, to, depth) ?? new List<QuoteEntity>();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, int count, DateTime to, int depth)
        {
            return GetFeedSrcOrNull(symbolCode).QueryTicks(count, to, depth) ?? new List<QuoteEntity>();
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

    //internal class BacktesterBarFeed : IBacktesterFeed
    //{
    //    private IEnumerable<BarEntity> _bidStream;
    //    private IEnumerable<BarEntity> _askStream;
    //    private string _symbol;

    //    public BacktesterBarFeed(string symbol, IEnumerable<BarEntity> bidStream, IEnumerable<BarEntity> askStream)
    //    {
    //        _symbol = symbol;
    //        _bidStream = bidStream;
    //        _askStream = askStream;
    //    }

    //    public IEnumerable<QuoteEntity> GetQuoteStream()
    //    {
    //        if (_bidStream != null)
    //        {
    //            if (_askStream != null)
    //            {
    //                return _bidStream.JoinSorted(_askStream, (b, a) => DateTime.Compare(b.OpenTime, a.OpenTime),
    //                    (b, a) => new QuoteEntity(_symbol, b?.OpenTime ?? a.OpenTime, b?.Open, a?.Open));
    //            }
    //            else
    //                return _bidStream.Select(b => new QuoteEntity(_symbol, b.OpenTime, b.Open, null));
    //        }
    //        else if (_askStream != null)
    //            return _bidStream.Select(b => new QuoteEntity(_symbol, b.OpenTime, null, b.Open));
    //        else
    //            throw new InvalidOperationException("Both ask and bid stream are null!");
    //    }
    //}

    //internal class BacktesterTickFeed : IBacktesterFeed
    //{
    //    private IEnumerable<QuoteEntity> _stream;

    //    public BacktesterTickFeed(IEnumerable<QuoteEntity> stream)
    //    {
    //        _stream = stream ?? throw new ArgumentNullException("stream");
    //    }

    //    public IEnumerable<QuoteEntity> GetQuoteStream()
    //    {
    //        return _stream;
    //    }
    //}
}
