using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class DictionarySelectorFull<TSrcKey, TResKey, TSource, TResult> : OperatorBase, IVarSet<TResKey, TResult>
    {
        private Dictionary<TResKey, TResult> snapshot = new Dictionary<TResKey, TResult>();
        private Dictionary<TSrcKey, TResKey> _keyMap = new Dictionary<TSrcKey, TResKey>();
        private IVarSet<TSrcKey, TSource> src;
        private Func<TSrcKey, TSource, KeyValuePair<TResKey, TResult>> selector;

        public DictionarySelectorFull(IVarSet<TSrcKey, TSource> src, Func<TSrcKey, TSource, KeyValuePair<TResKey, TResult>> selector)
        {
            this.src = src;
            this.selector = selector;

            foreach (var pair in src.Snapshot)
                Add(pair.Key, pair.Value);

            this.src.Updated += Src_Updated;
        }

        public IReadOnlyDictionary<TResKey, TResult> Snapshot { get { return snapshot; } }

        public event DictionaryUpdateHandler<TResKey, TResult> Updated;

        private void OnUpdated(DictionaryUpdateArgs<TResKey, TResult> args)
        {
            if (Updated != null)
                Updated(args);
        }

        private void Add(TSrcKey key, TSource value)
        {
            var selected = selector(key, value);
            _keyMap[key] = selected.Key;
            snapshot.Add(selected.Key, selected.Value);
            OnUpdated(new DictionaryUpdateArgs<TResKey, TResult>(this, DLinqAction.Insert, selected.Key, selected.Value));
        }

        private void Src_Updated(DictionaryUpdateArgs<TSrcKey, TSource> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
            {
                Add(args.Key, args.NewItem);
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var key = _keyMap[args.Key];
                var oldItem = snapshot[key];
                snapshot.Remove(key);
                _keyMap.Remove(args.Key);
                OnUpdated(new DictionaryUpdateArgs<TResKey, TResult>(this, DLinqAction.Remove, key, default(TResult), oldItem));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                var oldKey = _keyMap[args.Key];
                var oldVal = snapshot[oldKey];
                var newItem = selector(args.Key, args.NewItem);

                if (newItem.Key.Equals(oldKey))
                {
                    // replace
                    snapshot[oldKey] = newItem.Value;
                    OnUpdated(new DictionaryUpdateArgs<TResKey, TResult>(this, DLinqAction.Replace, oldKey, newItem.Value, oldVal));
                }
                else
                {
                    // remove
                    snapshot.Remove(oldKey);
                    OnUpdated(new DictionaryUpdateArgs<TResKey, TResult>(this, DLinqAction.Remove, oldKey, default(TResult), oldVal));

                    // add
                    snapshot.Add(newItem.Key, newItem.Value);
                    OnUpdated(new DictionaryUpdateArgs<TResKey, TResult>(this, DLinqAction.Insert, newItem.Key, newItem.Value));

                    _keyMap[args.Key] = newItem.Key;
                }
            }
        }

        protected override void DoDispose()
        {
            src.Updated -= Src_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
            OnUpdated(new DictionaryUpdateArgs<TResKey, TResult>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            src.Dispose();
        }
    }
}
