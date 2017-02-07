using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class DictionarySelector<TKey, TSource, TResult> : OperatorBase, IDynamicDictionarySource<TKey, TResult>
    {
        private Dictionary<TKey, TResult> snapshot = new Dictionary<TKey, TResult>();
        private IDynamicDictionarySource<TKey, TSource> src;
        private Func<TKey, TSource, TResult> selector;

        public DictionarySelector(IDynamicDictionarySource<TKey, TSource> src, Func<TKey, TSource, TResult> selector)
        {
            this.src = src;
            this.selector = selector;

            foreach (KeyValuePair<TKey, TSource> pair in src.Snapshot)
                Add(pair.Key, pair.Value);

            this.src.Updated += Src_Updated;
        }

        public IReadOnlyDictionary<TKey, TResult> Snapshot { get { return snapshot; } }

        public event DictionaryUpdateHandler<TKey, TResult> Updated;

        private void OnUpdated(DictionaryUpdateArgs<TKey, TResult> args)
        {
            if (Updated != null)
                Updated(args);
        }

        private void Add(TKey key, TSource value)
        {
            var selectedValue = selector(key, value);
            snapshot.Add(key, selectedValue);
            OnUpdated(new DictionaryUpdateArgs<TKey, TResult>(this, DLinqAction.Insert, key, selectedValue));
        }

        private void Src_Updated(DictionaryUpdateArgs<TKey, TSource> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
            {
                Add(args.Key, args.NewItem);
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var oldItem = snapshot[args.Key];
                snapshot.Remove(args.Key);
                OnUpdated(new DictionaryUpdateArgs<TKey, TResult>(this, DLinqAction.Remove, args.Key, default(TResult), oldItem));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                var oldItem = snapshot[args.Key];
                var newItem = selector(args.Key, args.NewItem);
                snapshot[args.Key] = newItem;
                OnUpdated(new DictionaryUpdateArgs<TKey, TResult>(this, DLinqAction.Replace, args.Key, newItem, oldItem));
            }
        }

        protected override void DoDispose()
        {
            src.Updated -= Src_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
            OnUpdated(new DictionaryUpdateArgs<TKey, TResult>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            src.Dispose();
        }
    }
}
