using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public class PluginFeedProvider : CrossDomainObject, IFeedProvider, IFeedHistoryProvider, IPluginMetadata, ISynchronizationContext
    {
        private ISyncContext _sync;
        private IFeedSubscription subscription;
        private IVarSet<string, SymbolInfo> symbols;
        private QuoteDistributor _distributor;
        private FeedHistoryProviderModel.Handler history;
        private Dictionary<string, int> _subscriptionCache;
        private IReadOnlyDictionary<string, CurrencyInfo> currencies;

        public event Action<QuoteEntity> RateUpdated;
        public event Action<List<QuoteEntity>> RatesUpdated { add { } remove { } }

        public ISynchronizationContext Sync { get { return this; } }

        public PluginFeedProvider(EntityCache cache, QuoteDistributor quoteDistributor, FeedHistoryProviderModel.Handler history, ISyncContext sync)
        {
            _sync = sync;
            this.symbols = cache.Symbols;
            _distributor = quoteDistributor;
            this.history = history;
            this.currencies = cache.Currencies.Snapshot;
            _subscriptionCache = new Dictionary<string, int>();
            subscription = quoteDistributor.AddSubscription(r => RateUpdated?.Invoke(r));
        }

        #region IFeedHistoryProvider implementation

        public List<BarEntity> QueryBars(string symbolCode, Api.BarPriceType priceType, DateTime from, DateTime to, Api.TimeFrames timeFrame)
        {
            return history.GetBarList(symbolCode, priceType, timeFrame, from, to).Result;
        }

        public List<BarEntity> QueryBars(string symbolCode, Api.BarPriceType priceType, DateTime from, int size, Api.TimeFrames timeFrame)
        {
            return history.GetBarPage(symbolCode, priceType, timeFrame, from, size).Result.ToList();
        }

        public List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, bool level2)
        {
            return history.GetQuoteList(symbolCode, from, to, level2).Result;
        }

        public List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, int count, bool level2)
        {
            return history.GetQuotePage(symbolCode, from, count, level2).Result.ToList();
        }

        #endregion

        #region IFeedProvider

        public QuoteEntity GetRate(string symbol)
        {
            throw new NotImplementedException();
        }

        public List<QuoteEntity> Modify(List<FeedSubscriptionUpdate> updates)
        {
            return subscription.Modify(updates);
        }

        public void CancelAll()
        {
            subscription.CancelAll();
        }

        public IEnumerable<QuoteEntity> GetSnapshot()
        {
            return symbols.Snapshot
                .Where(s => s.Value.LastQuote != null)
                .Select(s => s.Value.LastQuote).Cast<QuoteEntity>().ToList();
        }

        #endregion

        #region IPluginMetadata

        public IEnumerable<Domain.SymbolInfo> GetSymbolMetadata()
        {
            return symbols.Snapshot.Select(m => m.Value).ToList();
        }

        public IEnumerable<CurrencyInfo> GetCurrencyMetadata()
        {
            return currencies.Values.ToList();
        }

        #endregion

        #region ISynchronizationContext

        public void Invoke(Action action)
        {
            _sync.Invoke(action);
        }

        public void Send(Action action)
        {
            _sync.Send(action);
        }

        public void Invoke<T>(Action<T> action, T arg)
        {
            _sync.Invoke(action, arg);
        }

        #endregion
    }
}
