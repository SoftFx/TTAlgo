using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Server
{
    /// <summary>
    /// Not thread-safe.
    /// </summary>
    public class MessageCache<T>
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

        public List<T> GetCachedMessages(Func<T, bool> selector, int maxCount)
        {
            return _buffer.Where(selector).Take(maxCount).ToList();
        }

        public void SendSnapshot(ChannelWriter<T> sink)
        {
            foreach (var item in _buffer)
                sink.TryWrite(item);
        }
    }
}
