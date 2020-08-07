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
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTradeHistory(null, null, Domain.TradeHistoryRequestOptions.Backwards))).GetEnumerator();
        }

        public IEnumerable<TradeReport> Get(ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTradeHistory(null, null, options.ToDomainEnum())));
        }

        public IEnumerable<TradeReport> GetRange(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTradeHistory(from, to, options.ToDomainEnum())));
        }

        public IEnumerable<TradeReport> GetRange(DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTradeHistory(null, to, options.ToDomainEnum())));
        }

        public IAsyncEnumerator<TradeReport> GetAsync(ThQueryOptions options = ThQueryOptions.None)
        {
            return AdaptAsync(Provider?.GetTradeHistory(null, null, options.ToDomainEnum()));
        }

        public IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return AdaptAsync(Provider?.GetTradeHistory(from, to, options.ToDomainEnum()));
        }

        public IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return AdaptAsync(Provider?.GetTradeHistory(null, to, options.ToDomainEnum()));
        }

        private IEnumerable<TradeReport> Adapt(IEnumerable<Domain.TradeReportInfo> src)
        {
            return src.Select(e => new TradeReportAdapter(e, _symbols.GetOrNull(e.Symbol).Info));
        }

        private IAsyncEnumerator<TradeReport> AdaptAsync(IAsyncPagedEnumerator<Domain.TradeReportInfo> src)
        {
            return src.AsAsync().Select(e => (TradeReport)new TradeReportAdapter(e, _symbols.GetOrNull(e.Symbol).Info));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
