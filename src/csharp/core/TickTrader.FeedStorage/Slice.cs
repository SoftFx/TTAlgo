using System;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    internal class SliceInfo : ISliceInfo
    {
        public DateTime From { get; }

        public DateTime To { get; }
        public int Count { get; }


        public SliceInfo(DateTime from, DateTime to, int count)
        {
            From = from;
            To = to;
            Count = count;
        }
    }

    internal sealed class Slice<T> : SliceInfo
    {
        public T[] Items { get; }


        public Slice(DateTime from, DateTime to, T[] list) : base(from, to, list.Length)
        {
            Items = list;
        }
    }
}
