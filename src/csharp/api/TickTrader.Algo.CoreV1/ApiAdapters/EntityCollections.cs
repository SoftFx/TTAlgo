using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public abstract class TradeEntityCollection<CoreType, ApiType> : IEnumerable<ApiType> where CoreType : ApiType
    {
        protected readonly ConcurrentDictionary<string, CoreType> _entities = new ConcurrentDictionary<string, CoreType>();
        protected readonly PluginBuilder _builder;

        public TradeEntityCollection() { }

        public TradeEntityCollection(PluginBuilder builder)
        {
            _builder = builder;
        }

        public int Count => _entities.Count;

        public void Clear() => _entities.Clear();

        public virtual CoreType GetOrNull(string key) => _entities.GetOrDefault(key);

        public virtual IEnumerable<CoreType> Values => _entities.Values;

        IEnumerator<ApiType> IEnumerable<ApiType>.GetEnumerator() => _entities.Values.Select(u => (ApiType)u).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entities.Values.GetEnumerator();
    }


    public abstract class SymbolEntityBaseCollection<CoreType, InfoType, ApiType, NullType> : TradeEntityCollection<CoreType, ApiType>, IEnumerable<ApiType>
        where CoreType : class, ApiType, IBaseSymbolAccessor<InfoType>
        where InfoType : class, IBaseSymbolInfo
        where NullType : ApiType, new()
    {
        public ApiType this[string key] => _entities.TryGetValue(key, out CoreType entity) ? (ApiType)entity : new NullType();

        public void Init(IEnumerable<InfoType> newEntities)
        {
            _entities.Clear();

            newEntities?.ForEach(Add);
        }

        public override CoreType GetOrNull(string key)
        {
            var entity = base.GetOrNull(key);

            return (entity?.IsNull ?? true) ? null : entity;
        }

        public void AddOrUpdate(InfoType newInfo)
        {
            var oldEntity = base.GetOrNull(newInfo.Name);

            if (oldEntity == null)
                Add(newInfo);
            else
                oldEntity.Update(newInfo);
        }

        public void InvalidateAll() => Values.ForEach(u => u.Update(null));

        public void Merge(IEnumerable<InfoType> newInformation)
        {
            if (newInformation == null)
                return;

            InvalidateAll();
            newInformation.ForEach(AddOrUpdate);
        }

        public override IEnumerable<CoreType> Values => _entities.Values.Where(e => !e.IsNull).OrderBy(s => s.Info.GroupSortOrder).ThenBy(s => s.Info.SortOrder).ThenBy(s => s.Info.Name);

        public abstract void Add(InfoType info);

        public IEnumerator<ApiType> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
