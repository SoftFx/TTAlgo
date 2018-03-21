using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class DictionaryGrouping<TKey, TValue, TGrouping> : OperatorBase,
        IVarSet<TGrouping, IVarGrouping<TKey, TValue, TGrouping>>
    {
        private static readonly IEqualityComparer<TGrouping> groupKeyComparer = EqualityComparer<TGrouping>.Default;

        private VarDictionary<TGrouping, IVarGrouping<TKey, TValue, TGrouping>> groups
            = new VarDictionary<TGrouping, IVarGrouping<TKey, TValue, TGrouping>>();

        private Dictionary<TKey, TGrouping> groupingIndex = new Dictionary<TKey, TGrouping>();
        private IVarSet<TKey, TValue> src;
        private Func<TKey, TValue, TGrouping> selector;

        public event DictionaryUpdateHandler<TGrouping, IVarGrouping<TKey, TValue, TGrouping>> Updated
        {
            add { groups.Updated += value; }
            remove { groups.Updated -= value; }
        }

        public IReadOnlyDictionary<TGrouping, IVarGrouping<TKey, TValue, TGrouping>> Snapshot { get { return groups.Snapshot; } }

        public DictionaryGrouping(IVarSet<TKey, TValue> src, Func<TKey, TValue, TGrouping> selector)
        {
            this.src = src;
            this.selector = selector;

            foreach (var pair in src.Snapshot)
                Add(pair.Key, pair.Value);

            src.Updated += Src_Updated;
        }

        private void Src_Updated(DictionaryUpdateArgs<TKey, TValue> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
                Add(args.Key, args.NewItem);
            else if (args.Action == DLinqAction.Remove)
                Remove(args.Key);
            else if (args.Action == DLinqAction.Replace)
                Replace(args.Key, args.NewItem);
        }

        private Grouping GetOrAddGroup(TGrouping groupKey)
        {
            IVarGrouping<TKey, TValue, TGrouping> group;

            if (!groups.TryGetValue(groupKey, out group))
            {
                group = new Grouping(groupKey);
                groups.Add(groupKey, group);
            }
            return (Grouping)group;
        }

        private void Add(TKey key, TValue value)
        {
            TGrouping groupKey = selector(key, value);
            var group = GetOrAddGroup(groupKey);
            groupingIndex.Add(key, groupKey);
            group.Add(key, value);
        }

        private void Remove(TKey key)
        {
            var groupKey = groupingIndex[key];
            Remove(groupKey, key);
        }

        private void Remove(TGrouping groupKey, TKey key)
        {
            var group = groups[groupKey];

            groupingIndex.Remove(key);
            ((Grouping)group).RemoveOrThrow(key);

            if (((Grouping)group).Count == 0)
                groups.Remove(groupKey);
        }

        private void Replace(TKey key, TValue newValue)
        {
            var oldGroupKey = groupingIndex[key];
            var oldGroup = groups[oldGroupKey];
            var newGroupKey = selector(key, newValue);

            if (groupKeyComparer.Equals(oldGroupKey, newGroupKey))
                ((Grouping)oldGroup)[key] = newValue;
            else
            {
                Remove(oldGroupKey, key);

                var newGroup = GetOrAddGroup(newGroupKey);
                groupingIndex.Add(key, newGroupKey);
                newGroup.Add(key, newValue);
            }
        }

        protected override void DoDispose()
        {
            src.Updated += Src_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
        }

        protected override void SendDisposeToSources()
        {
        }

        private class Grouping : VarDictionary<TKey, TValue>, IVarGrouping<TKey, TValue, TGrouping>
        {
            public Grouping(TGrouping groupKey)
            {
                GroupKey = groupKey;
            }

            public TGrouping GroupKey { get; private set; }

            public void RemoveOrThrow(TKey key)
            {
                if (!Remove(key))
                    throw new Exception("Cannot find such key in group!");
            }
        }
    }
}
