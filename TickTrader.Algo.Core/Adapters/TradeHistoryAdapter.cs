using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class TradeHistoryAdapter : TradeHistory
    {
        private SymbolsCollection _symbols;

        public TradeHistoryAdapter(ITradeHistoryProvider provider, SymbolsCollection symbols)
        {
            Provider = provider;
            _symbols = symbols;
        }

        public ITradeHistoryProvider Provider { get; }

        public IEnumerator<TradeReport> GetEnumerator()
        {
            return Adapt(AsyncEnumerator.ToEnumerable(() => Provider?.GetTradeHistory(ThQueryOptions.Backwards))).GetEnumerator();
        }

        public IEnumerable<TradeReport> Get(ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.ToEnumerable(() => Provider?.GetTradeHistory(options)));
        }

        public IEnumerable<TradeReport> Get(int count, ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.ToEnumerable(() => Provider?.GetTradeHistory(count, options)));
        }

        public IEnumerable<TradeReport> GetRange(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.ToEnumerable(() => Provider?.GetTradeHistory(from, to, options)));
        }

        public IEnumerable<TradeReport> GetRange(DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.ToEnumerable(() => Provider?.GetTradeHistory(to, options)));
        }

        public IAsyncEnumerator<TradeReport> GetAsync(ThQueryOptions options = ThQueryOptions.None)
        {
            return AdaptAsync(Provider?.GetTradeHistory(options));
        }

        public IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return AdaptAsync(Provider?.GetTradeHistory(from, to, options));
        }

        public IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return AdaptAsync(Provider?.GetTradeHistory(to, options));
        }

        private IEnumerable<TradeReport> Adapt(IEnumerable<TradeReportEntity> src)
        {
            return src.Select(e => new TradeReportAdapter(e, _symbols.GetOrDefault(e.Symbol)));
        }

        private IAsyncEnumerator<TradeReport> AdaptAsync(IAsyncCrossDomainEnumerator<TradeReportEntity> src)
        {
            return src.AsAsync().Select(e => (TradeReport)new TradeReportAdapter(e, _symbols.GetOrDefault(e.Symbol)));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
