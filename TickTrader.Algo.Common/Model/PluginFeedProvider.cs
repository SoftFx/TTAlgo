using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public class PluginFeedProvider : IFeedProvider, IFeedHistoryProvider, IPluginMetadata
    {
        private IFeedSubscription subscription;
        private IVarSet<string, SymbolInfo> symbols;
        private QuoteDistributor _distributor;
        private FeedHistoryProviderModel.Handler history;
        private Dictionary<string, int> _subscriptionCache;
        private IReadOnlyDictionary<string, CurrencyInfo> currencies;

        public event Action<QuoteInfo> RateUpdated;
        public event Action<List<QuoteInfo>> RatesUpdated { add { } remove { } }

        public ISyncContext Sync { get; }

        public PluginFeedProvider(EntityCache cache, QuoteDistributor quoteDistributor, FeedHistoryProviderModel.Handler history, ISyncContext sync)
        {
            Sync = sync;
            this.symbols = cache.Symbols;
            _distributor = quoteDistributor;
            this.history = history;
            this.currencies = cache.Currencies.Snapshot;
            _subscriptionCache = new Dictionary<string, int>();
            subscription = quoteDistributor.AddSubscription(r => RateUpdated?.Invoke(r));
        }

        #region IFeedHistoryProvider implementation

        public List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            return history.GetBarList(symbol, marketSide, timeframe, from, to).GetAwaiter().GetResult();
        }

        public List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count)
        {
            return history.GetBarPage(symbol, marketSide, timeframe, from, count).GetAwaiter().GetResult().ToList();
        }

        public List<QuoteInfo> QueryQuotes(string symbolCode, Timestamp from, Timestamp to, bool level2)
        {
            return history.GetQuoteList(symbolCode, from, to, level2).GetAwaiter().GetResult();
        }

        public List<QuoteInfo> QueryQuotes(string symbolCode, Timestamp from, int count, bool level2)
        {
            return history.GetQuotePage(symbolCode, from, count, level2).GetAwaiter().GetResult().ToList();
        }

        public Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            return history.GetBarList(symbol, marketSide, timeframe, from, to);
        }

        public async Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count)
        {
            return (await history.GetBarPage(symbol, marketSide, timeframe, from, count)).ToList();
        }

        public Task<List<QuoteInfo>> QueryQuotesAsync(string symbolCode, Timestamp from, Timestamp to, bool level2)
        {
            return history.GetQuoteList(symbolCode, from, to, level2);
        }

        public async Task<List<QuoteInfo>> QueryQuotesAsync(string symbolCode, Timestamp from, int count, bool level2)
        {
            return (await history.GetQuotePage(symbolCode, from, count, level2)).ToList();
        }

        #endregion

        #region IFeedProvider

        public QuoteInfo GetRate(string symbol)
        {
            throw new NotImplementedException();
        }

        public List<QuoteInfo> Modify(List<FeedSubscriptionUpdate> updates)
        {
            return subscription.Modify(updates);
        }

        public Task<List<QuoteInfo>> ModifyAsync(List<FeedSubscriptionUpdate> updates)
        {
            return Task.FromResult(Modify(updates));
        }

        public void CancelAll()
        {
            subscription.CancelAll();
        }

        public Task CancelAllAsync()
        {
            CancelAll();
            return Task.CompletedTask;
        }

        public List<QuoteInfo> GetSnapshot()
        {
            return symbols.Snapshot
                .Where(s => s.Value.LastQuote != null)
                .Select(s => s.Value.LastQuote).Cast<QuoteInfo>().ToList();
        }

        public Task<List<QuoteInfo>> GetSnapshotAsync()
        {
            return Task.FromResult(GetSnapshot());
        }

        public IFeedSubscription GetSubscription()
        {
            return this;
        }

        #endregion

        #region IPluginMetadata

        public IEnumerable<Domain.SymbolInfo> GetSymbolMetadata()
        {
            return symbols.Snapshot.Values.ToList();
        }

        public IEnumerable<CurrencyInfo> GetCurrencyMetadata()
        {
            return currencies.Values.ToList();
        }

        public IEnumerable<FullQuoteInfo> GetLastQuoteMetadata()
        {
            return symbols.Snapshot.Values.Where(u => u.LastQuote != null).Select(u => (u.LastQuote as QuoteInfo)?.GetFullQuote()).ToList();
        }

        #endregion
    }
}
