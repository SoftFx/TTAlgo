using System.Collections;
using System.Collections.Generic;

namespace TickTrader.Algo.Core.Lib
{
    /// <summary>
    /// Not thread-safe.
    /// </summary>
    public class ItemCache<T> : IEnumerable<T>
    {
        private readonly CircularList<T> _buffer;
        private readonly int _maxCachedRecords;


        public ItemCache(int maxCachedRecords)
        {
            _maxCachedRecords = maxCachedRecords;
            _buffer = new CircularList<T>(maxCachedRecords);
        }


        public void Clear() => _buffer.Clear();

        public void Add(T item)
        {
            if (_maxCachedRecords != -1 && _buffer.Count >= _maxCachedRecords)
                _buffer.Dequeue();

            _buffer.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                Add(item);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _buffer.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _buffer.GetEnumerator();
    }
}
