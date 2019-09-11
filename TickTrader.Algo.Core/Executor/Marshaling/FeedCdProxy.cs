using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class FeedCdProxy : CrossDomainObject, IFeedProvider, IFeedHistoryProvider
    {
        private BunchingBlock<QuoteEntity> _bBlock;
        private readonly IFeedProvider _feed;
        private readonly IFeedHistoryProvider _history;

        public FeedCdProxy(IFeedProvider feed, IFeedHistoryProvider history)
        {
            _feed = feed;
            _history = history;
        }

        public void Stop()
        {
            _feed.RateUpdated -= _feed_RateUpdated;
            _bBlock.Complete();
            _bBlock.Completion.Wait();
        }

        public ISynchronizationContext Sync => _feed.Sync;

        public event Action<QuoteEntity> RateUpdated { add { } remove { } }
        public event Action<List<QuoteEntity>> RatesUpdated;

        public IEnumerable<QuoteEntity> GetSnapshot()
        {
            return _feed.GetSnapshot();
        }

        //public QuoteEntity GetRate(string symbol)
        //{
        //    return _feed.GetRate(symbol);
        //}

        public List<BarEntity> QueryBars(string symbolCode, BarPriceType priceType, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return _history.QueryBars(symbolCode, priceType, from, to, timeFrame);
        }

        public List<BarEntity> QueryBars(string symbolCode, BarPriceType priceType, DateTime from, int size, TimeFrames timeFrame)
        {
            return _history.QueryBars(symbolCode, priceType, from, size, timeFrame);
        }

        public List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, bool level2)
        {
            return _history.QueryTicks(symbolCode, from, to, level2);
        }

        public List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, int count, bool level2)
        {
            return _history.QueryTicks(symbolCode, from, count, level2);
        }

        //public void Modify(FeedSubscriptionUpdate update)
        //{
        //    _feed.Modify(update);
        //}

        public List<QuoteEntity> Modify(List<FeedSubscriptionUpdate> updates)
        {
            return _feed.Modify(updates);
        }

        //public void SubscribeForAll()
        //{
        //    _feed.SubscribeForAll();
        //}

        public void CancelAll()
        {
            _feed.CancelAll();
        }

        //public void Subscribe(string symbol, int depth)
        //{
        //    _feed.Subscribe(symbol, depth);
        //}

        //public void SubscribeForAll()
        //{
        //    _feed.SubscribeForAll();
        //}

        //public void UnsubscribeAll()
        //{
        //    if (_bBlock != null)
        //    {
        //        _bBlock.Complete(true);
        //        _feed.RateUpdated += _feed_RateUpdated;
        //        _feed.RatesUpdated += _feed_RatesUpdated;
        //        _feed.UnsubscribeAll();
        //    }
        //}

        private void LazyInit()
        {
            if (_bBlock == null)
            {
                _bBlock = new BunchingBlock<QuoteEntity>(TransferToCore, 30);
                _feed.RateUpdated += _feed_RateUpdated;
                _feed.RatesUpdated += _feed_RatesUpdated;
            }
        }

        private void TransferToCore(List<QuoteEntity> page)
        {
            RatesUpdated?.Invoke(page);
        }

        private void _feed_RatesUpdated(List<QuoteEntity> obj)
        {
        }

        private void _feed_RateUpdated(QuoteEntity obj)
        {
        }
    }
}
