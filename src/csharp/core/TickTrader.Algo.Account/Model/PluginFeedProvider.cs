using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class PluginFeedProvider : IFeedProvider, IFeedHistoryProvider, IPluginMetadata
    {
        private readonly IQuoteSub _quoteSub;
        private readonly IQuoteSubManager _quoteSubManager;
        private readonly IBarSub _barSub;
        private readonly IBarSubManager _barSubManager;
        private readonly IDisposable _quoteSubHandler, _barSubHandler;
        private readonly IVarSet<string, SymbolInfo> _symbols;
        private readonly FeedHistoryProviderModel.Handler _history;
        private readonly IReadOnlyDictionary<string, CurrencyInfo> _currencies;

        public event Action<QuoteInfo> QuoteUpdated;
        public event Action<BarUpdate> BarUpdated;


        public PluginFeedProvider(EntityCache cache, IQuoteSubManager quoteSubManager, IBarSubManager barSubManager, FeedHistoryProviderModel.Handler history)
        {
            _symbols = cache.Symbols;
            _history = history;
            _currencies = cache.Currencies.Snapshot;
            _quoteSubManager = quoteSubManager;
            _barSubManager = barSubManager;

            _quoteSub = new QuoteSubscription(quoteSubManager);
            _quoteSubHandler = _quoteSub.AddHandler(r => QuoteUpdated?.Invoke(r));

            _barSub = new BarSubscription(barSubManager);
            _barSubHandler = _barSub.AddHandler(r => BarUpdated?.Invoke(r));
        }

        #region IFeedHistoryProvider implementation

        public List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, UtcTicks to)
        {
            return _history.GetBarList(symbol, marketSide, timeframe, from, to).GetAwaiter().GetResult();
        }

        public List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, int count)
        {
            return _history.GetBarPage(symbol, marketSide, timeframe, from, count).GetAwaiter().GetResult().ToList();
        }

        public List<QuoteInfo> QueryQuotes(string symbolCode, UtcTicks from, UtcTicks to, bool level2)
        {
            return _history.GetQuoteList(symbolCode, from, to, level2).GetAwaiter().GetResult();
        }

        public List<QuoteInfo> QueryQuotes(string symbolCode, UtcTicks from, int count, bool level2)
        {
            return _history.GetQuotePage(symbolCode, from, count, level2).GetAwaiter().GetResult().ToList();
        }

        public Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, UtcTicks to)
        {
            return _history.GetBarList(symbol, marketSide, timeframe, from, to);
        }

        public async Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, int count)
        {
            return (await _history.GetBarPage(symbol, marketSide, timeframe, from, count)).ToList();
        }

        public Task<List<QuoteInfo>> QueryQuotesAsync(string symbolCode, UtcTicks from, UtcTicks to, bool level2)
        {
            return _history.GetQuoteList(symbolCode, from, to, level2);
        }

        public async Task<List<QuoteInfo>> QueryQuotesAsync(string symbolCode, UtcTicks from, int count, bool level2)
        {
            return (await _history.GetQuotePage(symbolCode, from, count, level2)).ToList();
        }

        #endregion

        #region IFeedProvider

        public List<QuoteInfo> GetQuoteSnapshot()
        {
            return _symbols.Snapshot
                .Where(s => s.Value.LastQuote != null)
                .Select(s => s.Value.LastQuote).Cast<QuoteInfo>().ToList();
        }

        public Task<List<QuoteInfo>> GetQuoteSnapshotAsync()
        {
            return Task.FromResult(GetQuoteSnapshot());
        }

        public IQuoteSub GetQuoteSub() => new QuoteSubscription(_quoteSubManager);

        public IBarSub GetBarSub() => new BarSubscription(_barSubManager);

        #endregion

        #region IPluginMetadata

        public IEnumerable<Domain.SymbolInfo> GetSymbolMetadata()
        {
            return _symbols.Snapshot.Values.ToList();
        }

        public IEnumerable<CurrencyInfo> GetCurrencyMetadata()
        {
            return _currencies.Values.ToList();
        }

        public IEnumerable<FullQuoteInfo> GetLastQuoteMetadata()
        {
            return _symbols.Snapshot.Values.Where(u => u.LastQuote != null).Select(u => (u.LastQuote as QuoteInfo)?.GetFullQuote()).ToList();
        }

        #endregion
    }
}
