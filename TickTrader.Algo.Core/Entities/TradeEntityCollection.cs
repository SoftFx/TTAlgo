using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Core
{
    internal abstract class TradeEntityCollection<CoreType, ApiType> : IEnumerable<ApiType> where CoreType : ApiType
    {
        protected readonly ConcurrentDictionary<string, CoreType> _entities = new ConcurrentDictionary<string, CoreType>();
        protected readonly PluginBuilder _builder;

        public TradeEntityCollection(PluginBuilder builder)
        {
            _builder = builder;
        }

        public int Count => _entities.Count;

        public void Clear() => _entities.Clear();

        public CoreType GetOrNull(string key) => _entities.GetOrDefault(key);

        public IEnumerable<CoreType> Values => _entities.Values;

        IEnumerator<ApiType> IEnumerable<ApiType>.GetEnumerator() => _entities.Values.Select(u => (ApiType)u).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entities.Values.GetEnumerator();
    }
}
