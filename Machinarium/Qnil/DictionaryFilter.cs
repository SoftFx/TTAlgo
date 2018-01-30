using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class DictionaryFilter<TKey, TValue> : OperatorBase, IVarSet<TKey, TValue>
    {
        private Dictionary<TKey, TValue> snapshot = new Dictionary<TKey, TValue>();
        private IVarSet<TKey, TValue> src;
        private Func<TKey, TValue, bool> condition;

        public DictionaryFilter(IVarSet<TKey, TValue> src, Func<TKey, TValue, bool> condition)
        {
            this.src = src;
            this.condition = condition;

            foreach (KeyValuePair<TKey, TValue> pair in src.Snapshot)
            {
                if (condition(pair.Key, pair.Value))
                    snapshot.Add(pair.Key, pair.Value);
            }

            src.Updated += Src_Updated;
        }

        public IReadOnlyDictionary<TKey, TValue> Snapshot { get { return snapshot; } }

        public event DictionaryUpdateHandler<TKey, TValue> Updated;

        private void OnUpdated(DictionaryUpdateArgs<TKey, TValue> args)
        {
            if (Updated != null)
                Updated(args);
        }

        private void Add(TKey key, TValue value)
        {
            snapshot.Add(key, value);
            OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Insert, key, value));
        }

        private void Src_Updated(DictionaryUpdateArgs<TKey, TValue> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
            {
                if (condition(args.Key, args.NewItem))
                    Add(args.Key, args.NewItem);
            }
            else if (args.Action == DLinqAction.Remove)
            {
                if (snapshot.Remove(args.Key))
                    OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Remove, args.Key, default(TValue), args.OldItem));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                bool itemInSnapshot = snapshot.ContainsKey(args.Key);
                bool newItemMetCondition = condition(args.Key, args.NewItem);

                if (itemInSnapshot)
                {
                    if (newItemMetCondition)
                    {
                        // replace
                        snapshot[args.Key] = args.NewItem;
                        OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Replace, args.Key, args.NewItem, args.OldItem));
                    }
                    else
                    {
                        // remove
                        snapshot.Remove(args.Key);
                        OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Remove, args.Key, default(TValue), args.OldItem));
                    }
                }
                else
                {
                    if (newItemMetCondition)
                    {
                        // insert
                        Add(args.Key, args.NewItem);
                    }
                    else
                    {
                        // do nothing
                    }
                }
            }
        }

        protected override void DoDispose()
        {
            src.Updated -= Src_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
            OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            src.Dispose();
        }
    }
}
