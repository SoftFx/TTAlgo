using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface TradeHistory : IEnumerable<TradeReport>
    {
        IEnumerable<TradeReport> Get(bool skipCancelOrders = false);
        IEnumerable<TradeReport> GetRange(DateTime from, DateTime to, bool skipCancelOrders = false);
        IEnumerable<TradeReport> GetRange(DateTime to, bool skipCancelOrders = false);
        IAsyncEnumerator<TradeReport[]> GetAsync(bool skipCancelOrders = false);
        IAsyncEnumerator<TradeReport[]> GetRangeAsync(DateTime from, DateTime to, bool skipCancelOrders = false);
        IAsyncEnumerator<TradeReport[]> GetRangeAsync(DateTime to, bool skipCancelOrders = false);
    }

    public interface IAsyncEnumerator<T> : IDisposable
    {
        Task<bool> Next();
        T Current { get; }
    }
}
