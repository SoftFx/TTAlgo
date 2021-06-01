using System;

namespace TickTrader.Algo.Account.FeedStorage
{
    public class SliceInfo
    {
        public SliceInfo(DateTime from, DateTime to, int count)
        {
            From = from;
            To = to;
            Count = count;
        }

        public int Count { get; }
        public DateTime From { get; }
        public DateTime To { get; }
    }

    public class Slice<T> : SliceInfo
    {
        public Slice(DateTime from, DateTime to, T[] list)
            : base(from, to, list.Length)
        {
            Items = list;
        }

        public T[] Items { get; }
    }
}
