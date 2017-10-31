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
        private ITradeHistoryProvider _provider;

        public ITradeHistoryProvider Provider { get { return _provider; } set { _provider = value; } }

        public IEnumerator<TradeReport> GetEnumerator()
        {
            return AsyncEnumerator.ToEnumerable(() => _provider?.GetTradeHistory(false)).GetEnumerator();
        }

        public IEnumerable<TradeReport> Get(bool skipCancelOrders = false)
        {
            return AsyncEnumerator.ToEnumerable(() => _provider?.GetTradeHistory(skipCancelOrders));
        }

        public IEnumerable<TradeReport> GetRange(DateTime from, DateTime to, bool skipCancelOrders)
        {
            return AsyncEnumerator.ToEnumerable(() => _provider?.GetTradeHistory(from, to, skipCancelOrders));
        }

        public IEnumerable<TradeReport> GetRange(DateTime to, bool skipCancelOrders)
        {
            return AsyncEnumerator.ToEnumerable(() => _provider?.GetTradeHistory(to, skipCancelOrders));
        }

        public IAsyncEnumerator<TradeReport[]> GetAsync(bool skipCancelOrders)
        {
            return _provider?.GetTradeHistory(skipCancelOrders).AsPagedAsync();
        }

        public IAsyncEnumerator<TradeReport[]> GetRangeAsync(DateTime from, DateTime to, bool skipCancelOrders)
        {
            return _provider?.GetTradeHistory(from, to, skipCancelOrders).AsPagedAsync();
        }

        public IAsyncEnumerator<TradeReport[]> GetRangeAsync(DateTime to, bool skipCancelOrders)
        {
            return _provider?.GetTradeHistory(to, skipCancelOrders).AsPagedAsync();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
