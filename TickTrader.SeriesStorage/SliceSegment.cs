using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    internal class SliceSegment<TKey, TValue> : ISlice<TKey, TValue>
    {
        public SliceSegment(TKey from, TKey to, ArraySegment<TValue> content)
        {
            if (content.Array == null)
                throw new ArgumentException();

            From = from;
            To = to;
            Content = content;
        }

        public TKey From { get; }
        public TKey To { get; }
        public ArraySegment<TValue> Content { get; }
        public bool IsEmpty => Content.Count == 0;
        public bool IsMissing => false;
    }
}
