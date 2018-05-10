using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    class FeedEmulator : CrossDomainObject, IPluginFeedProvider, ISynchronizationContext
    {
        private Dictionary<string, IBacktesterFeed> _feedSources = new Dictionary<string, IBacktesterFeed>();

        ISynchronizationContext IPluginFeedProvider.Sync => this;

        public IPagedEnumerator<QuoteEntity> GetFeedStream()
        {
            return GetJoinedStream().GetCrossDomainEnumerator(8000);
        }

        private IEnumerable<QuoteEntity> GetJoinedStream()
        {
            var streams = _feedSources.Values.Select(s => s.GetQuoteStream().GetEnumerator()).ToList();

            foreach (var e in streams.ToList())
            {
                if (!e.MoveNext())
                    streams.Remove(e);
            }

            while (streams.Count > 0)
            {
                var max = streams.MaxBy(e => e.Current.Time);
                var nextQuote = max.Current;

                if (!max.MoveNext())
                    streams.Remove(max);

                yield return nextQuote;
            }
        }

        public void AddFeedSource(string symbol, IBacktesterFeed source)
        {
            _feedSources.Add(symbol, source);
        }

        #region IPluginFeedProvider

        IEnumerable<QuoteEntity> IPluginFeedProvider.GetSnapshot()
        {
            return new List<QuoteEntity>();
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return new List<BarEntity>();
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, int size, TimeFrames timeFrame)
        {
            return new List<BarEntity>();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, DateTime to, int depth)
        {
            return new List<QuoteEntity>();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, int count, DateTime to, int depth)
        {
            return new List<QuoteEntity>();
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

    internal interface IBacktesterFeed
    {
        IEnumerable<QuoteEntity> GetQuoteStream();
    }

    internal class BacktesterBarFeed : IBacktesterFeed
    {
        private IEnumerable<BarEntity> _bidStream;
        private IEnumerable<BarEntity> _askStream;
        private string _symbol;

        public BacktesterBarFeed(string symbol, IEnumerable<BarEntity> bidStream, IEnumerable<BarEntity> askStream)
        {
            _symbol = symbol;
            _bidStream = bidStream;
            _askStream = askStream;
        }

        public IEnumerable<QuoteEntity> GetQuoteStream()
        {
            if (_bidStream != null)
            {
                if (_askStream != null)
                {
                    return _bidStream.JoinSorted(_askStream, (b, a) => DateTime.Compare(b.OpenTime, a.OpenTime),
                        (b, a) => new QuoteEntity(_symbol, b?.OpenTime ?? a.OpenTime, b?.Open, a?.Open));
                }
                else
                    return _bidStream.Select(b => new QuoteEntity(_symbol, b.OpenTime, b.Open, null));
            }
            else if (_askStream != null)
                return _bidStream.Select(b => new QuoteEntity(_symbol, b.OpenTime, null, b.Open));
            else
                throw new InvalidOperationException("Both ask and bid stream are null!");
        }
    }

    internal class BacktesterTickFeed : IBacktesterFeed
    {
        private IEnumerable<QuoteEntity> _stream;

        public BacktesterTickFeed(IEnumerable<QuoteEntity> stream)
        {
            _stream = stream ?? throw new ArgumentNullException("stream");
        }

        public IEnumerable<QuoteEntity> GetQuoteStream()
        {
            return _stream;
        }
    }
}
