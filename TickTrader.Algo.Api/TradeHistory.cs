using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface TradeHistory : IEnumerable<TradeReport>
    {
        IEnumerable<TradeReport> Get(ThQueryOptions options = ThQueryOptions.None);
        IEnumerable<TradeReport> GetRange(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None);
        IEnumerable<TradeReport> GetRange(DateTime to, ThQueryOptions options = ThQueryOptions.None);
        IAsyncEnumerator<TradeReport> GetAsync(ThQueryOptions options = ThQueryOptions.None);
        IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None);
        IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime to, ThQueryOptions options = ThQueryOptions.None);
    }

    [Flags]
    public enum ThQueryOptions
    {
        None            = 0x0,
        SkipCanceled    = 0x1,
        Backwards       = 0x2
    }


    public interface IAsyncEnumerator<T> : IDisposable
    {
        Task<bool> Next();
        T Current { get; }
    }
}
