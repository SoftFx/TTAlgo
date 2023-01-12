using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal sealed class TradeHistoryAdapter : IHistoryProvider
    {
        private readonly SymbolsCollection _symbols;

        public TradeHistoryAdapter(ITradeHistoryProvider provider, SymbolsCollection symbols)
        {
            Provider = provider;
            _symbols = symbols;
        }

        public ITradeHistoryProvider Provider { get; }

        public IEnumerator<TradeReport> GetEnumerator()
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTradeHistory(null, null, Domain.HistoryRequestOptions.Backwards))).GetEnumerator();
        }

        public IEnumerable<TradeReport> Get(ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTradeHistory(null, null, options.ToDomainEnum())));
        }

        public IEnumerable<TradeReport> GetRange(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTradeHistory(from.ToUtcTicks(), to.ToUtcTicks(), options.ToDomainEnum())));
        }

        public IEnumerable<TradeReport> GetRange(DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTradeHistory(null, to.ToUtcTicks(), options.ToDomainEnum())));
        }

        public Api.IAsyncEnumerator<TradeReport> GetAsync(ThQueryOptions options = ThQueryOptions.None)
        {
            return AdaptAsync(Provider?.GetTradeHistory(null, null, options.ToDomainEnum()));
        }

        public Api.IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return AdaptAsync(Provider?.GetTradeHistory(from.ToUtcTicks(), to.ToUtcTicks(), options.ToDomainEnum()));
        }

        public Api.IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime to, ThQueryOptions options = ThQueryOptions.None)
        {
            return AdaptAsync(Provider?.GetTradeHistory(null, to.ToUtcTicks(), options.ToDomainEnum()));
        }

        private IEnumerable<TradeReport> Adapt(IEnumerable<Domain.TradeReportInfo> src)
        {
            return src.Select(e => new TradeReportAdapter(e, _symbols.GetOrNull(e.Symbol)?.Info));
        }

        private Api.IAsyncEnumerator<TradeReport> AdaptAsync(IAsyncPagedEnumerator<Domain.TradeReportInfo> src)
        {
            return src.AsAsync().Select(e => (TradeReport)new TradeReportAdapter(e, _symbols.GetOrNull(e.Symbol)?.Info));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        IEnumerable<TriggerReport> TriggerHistory.Get(ThQueryOptions options)
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTriggerHistory(null, null, options.ToDomainEnum())));
        }

        IEnumerable<TriggerReport> TriggerHistory.GetRange(DateTime from, DateTime to, ThQueryOptions options)
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTriggerHistory(from.ToUtcTicks(), to.ToUtcTicks(), options.ToDomainEnum())));
        }

        IEnumerable<TriggerReport> TriggerHistory.GetRange(DateTime to, ThQueryOptions options)
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTriggerHistory(null, to.ToUtcTicks(), options.ToDomainEnum())));
        }

        Api.IAsyncEnumerator<TriggerReport> TriggerHistory.GetAsync(ThQueryOptions options)
        {
            return AdaptAsync(Provider?.GetTriggerHistory(null, null, options.ToDomainEnum()));
        }

        Api.IAsyncEnumerator<TriggerReport> TriggerHistory.GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options)
        {
            return AdaptAsync(Provider?.GetTriggerHistory(from.ToUtcTicks(), to.ToUtcTicks(), options.ToDomainEnum()));
        }

        Api.IAsyncEnumerator<TriggerReport> TriggerHistory.GetRangeAsync(DateTime to, ThQueryOptions options)
        {
            return AdaptAsync(Provider?.GetTriggerHistory(null, to.ToUtcTicks(), options.ToDomainEnum()));
        }

        IEnumerator<TriggerReport> IEnumerable<TriggerReport>.GetEnumerator()
        {
            return Adapt(AsyncEnumerator.GetAdapter(() => Provider?.GetTriggerHistory(null, null, Domain.HistoryRequestOptions.Backwards))).GetEnumerator();
        }

        private IEnumerable<TriggerReport> Adapt(IEnumerable<Domain.TriggerReportInfo> src)
        {
            return src.Select(e => new TriggerReportAdapter(e, _symbols.GetOrNull(e.Symbol)?.Info));
        }

        private Api.IAsyncEnumerator<TriggerReport> AdaptAsync(IAsyncPagedEnumerator<Domain.TriggerReportInfo> src)
        {
            return src.AsAsync().Select(e => (TriggerReport)new TriggerReportAdapter(e, _symbols.GetOrNull(e.Symbol)?.Info));
        }
    }
}