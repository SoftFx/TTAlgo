using System.Collections.Generic;

namespace TickTrader.Algo.Core.Lib
{
    /// <summary>
    /// Not thread-safe.
    /// </summary>
    public class CircularItemCache<T> : CircularList<T>
    {
        public int MaxRecords { get; set; }


        public CircularItemCache(int maxRecords)
            : base(maxRecords)
        {
            MaxRecords = maxRecords;
        }


        public override void Add(T item)
        {
            if (MaxRecords != -1 && Count >= MaxRecords)
                Dequeue();

            base.Add(item);
        }

        public override void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
    }
}
