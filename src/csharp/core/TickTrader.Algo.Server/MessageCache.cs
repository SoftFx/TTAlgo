using System.Collections;
using System.Collections.Generic;
using System.Threading.Channels;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Server
{
    /// <summary>
    /// Not thread-safe.
    /// </summary>
    public class MessageCache<T> : IEnumerable<T>
    {
        private readonly CircularList<T> _buffer = new CircularList<T>();
        private readonly int _maxCachedRecords;


        public MessageCache(int maxCachedRecords)
        {
            _maxCachedRecords = maxCachedRecords;
        }


        public void Clear() => _buffer.Clear();

        public void Add(T item)
        {
            if (_maxCachedRecords != -1 && _buffer.Count >= _maxCachedRecords)
                _buffer.Dequeue();

            _buffer.Add(item);
        }

        public void SendSnapshot(ChannelWriter<T> sink)
        {
            foreach (var item in _buffer)
                sink.TryWrite(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _buffer.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _buffer.GetEnumerator();
    }
}
