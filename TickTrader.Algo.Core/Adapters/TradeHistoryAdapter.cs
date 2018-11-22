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
        public TradeHistoryAdapter(ITradeHistoryProvider provider)
        {
            Provider = provider;
        }

        public ITradeHistoryProvider Provider { get; }

        public IEnumerator<TradeReport> GetEnumerator()
        {
            return AsyncEnumerator.ToEnumerable(() => Provider?.GetTradeHistory(false)).GetEnumerator();
        }

        public IEnumerable<TradeReport> Get(bool skipCancelOrders = false)
        {
            return AsyncEnumerator.ToEnumerable(() => Provider?.GetTradeHistory(skipCancelOrders));
        }

        public IEnumerable<TradeReport> GetRange(DateTime from, DateTime to, bool skipCancelOrders)
        {
            return AsyncEnumerator.ToEnumerable(() => Provider?.GetTradeHistory(from, to, skipCancelOrders));
        }

        public IEnumerable<TradeReport> GetRange(DateTime to, bool skipCancelOrders)
        {
            return AsyncEnumerator.ToEnumerable(() => Provider?.GetTradeHistory(to, skipCancelOrders));
        }

        public IAsyncEnumerator<TradeReport> GetAsync(bool skipCancelOrders)
        {
            return Provider?.GetTradeHistory(skipCancelOrders).AsAsync();
        }

        public IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime from, DateTime to, bool skipCancelOrders)
        {
            return Provider?.GetTradeHistory(from, to, skipCancelOrders).AsAsync();
        }

        public IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime to, bool skipCancelOrders)
        {
            return Provider?.GetTradeHistory(to, skipCancelOrders).AsAsync();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
