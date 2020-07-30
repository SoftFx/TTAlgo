using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TickTrader.Algo.Core
{
    internal sealed class TradeEntityCollection<T> : IEnumerable<T> where T: class
    {
        private readonly ConcurrentDictionary<string, T> _entities = new ConcurrentDictionary<string, T>();

        public T GetOrNull(string key) => _entities.GetOrDefault(key);

        public IEnumerator<T> GetEnumerator() => _entities.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
