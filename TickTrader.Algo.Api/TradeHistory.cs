using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface TradeHistory : IEnumerable<TradeReport>
    {
        IEnumerable<TradeReport> GetRange(DateTime from, DateTime to);
        IEnumerable<TradeReport> GetRange(DateTime to);
        IAsyncEnumerator<TradeReport> GetAsync();
        IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime from, DateTime to);
        IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime to);
    }

    public interface IAsyncEnumerator<T> : IDisposable where T : class
    {
        Task<T[]> GetNextPage();
    }
}
