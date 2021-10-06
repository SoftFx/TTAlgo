using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface IHistoryProvider : TradeHistory, TriggerHistory
    {

    }


    public interface TradeHistory : IEnumerable<TradeReport>
    {
        IEnumerable<TradeReport> Get(ThQueryOptions options = ThQueryOptions.None);
        IEnumerable<TradeReport> GetRange(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None);
        IEnumerable<TradeReport> GetRange(DateTime to, ThQueryOptions options = ThQueryOptions.None);
        IAsyncEnumerator<TradeReport> GetAsync(ThQueryOptions options = ThQueryOptions.None);
        IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None);
        IAsyncEnumerator<TradeReport> GetRangeAsync(DateTime to, ThQueryOptions options = ThQueryOptions.None);
    }

    public interface TriggerHistory : IEnumerable<TriggerReport>
    {
        IEnumerable<TriggerReport> Get(ThQueryOptions options = ThQueryOptions.None);
        IEnumerable<TriggerReport> GetRange(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None);
        IEnumerable<TriggerReport> GetRange(DateTime to, ThQueryOptions options = ThQueryOptions.None);
        IAsyncEnumerator<TriggerReport> GetAsync(ThQueryOptions options = ThQueryOptions.None);
        IAsyncEnumerator<TriggerReport> GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options = ThQueryOptions.None);
        IAsyncEnumerator<TriggerReport> GetRangeAsync(DateTime to, ThQueryOptions options = ThQueryOptions.None);
    }

    [Flags]
    public enum ThQueryOptions
    {
        None = 0x0,
        SkipCanceled = 0x1,
        Backwards = 0x2
    }


    public interface IAsyncEnumerator<T> : IDisposable
    {
        Task<bool> Next();
        T Current { get; }
    }
}
