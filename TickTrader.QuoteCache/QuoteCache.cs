using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.QuoteCache
{
    public class QuoteCache
    {
        public QuoteCache()
        {
        }
    }

    //public class BarPage
    //{
    //    public DateTime OpenTime { get; }
    //    public DateTime CloseTime { get; }
    //    public IList<Bar> Bars { get; }
    //}

    //public class BarCursor : IDisposable
    //{
    //    BarPage CurrentPage { get; }
    //    bool Next();
    //    bool Prev();
    //    bool SeekBefore(DateTime startTime);
    //    bool SeekAfter(DateTime startTime);
    //}

    public interface IFeedUpdateSource
    {
    }

    public interface IFeedFileSouce
    {

    }

    public interface FeedPage<T>
        where T : TimeBasedRecord
    {
        DateTime OpenTime { get; }
        DateTime CloseTime { get; }
        List<T> Records { get; }

        //event Action<T, int> Updated;
        //event Action<T, int> Addded;
        //event Action Closed;
    }

    public interface FeedCursor<T> : IDisposable
        where T : TimeBasedRecord
    {
        FeedPage<T> CurrentPage { get; }
        bool Next();
        bool Prev();
        bool SeekToStart();
        bool SeekToEnd();
        bool SeekBefore(DateTime startTime);
        bool SeekAfter(DateTime startTime);
    }

    public interface TimeBasedRecord
    {
        DateTime OpenTime { get; }
        DateTime CloseTime { get; }
    }

    internal interface FileChache
    {

    }
}
