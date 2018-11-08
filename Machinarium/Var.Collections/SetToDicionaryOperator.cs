using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class SetToDicionaryOperator<TKey, TValue> : OperatorBase, IVarSet<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _snapshot = new Dictionary<TKey, TValue>();
        private IVarSet<TKey> _srcSet;
        private Func<TKey, TValue> _selector;

        public SetToDicionaryOperator(IVarSet<TKey> srcSet, Func<TKey, TValue> selector)
        {
            _srcSet = srcSet;
            _selector = selector;

            foreach (var key in srcSet.Snapshot)
                _snapshot.Add(key, selector(key));

            srcSet.Updated += SrcSet_Updated;
        }

        private void SrcSet_Updated(SetUpdateArgs<TKey> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
            {
                var newKey = args.NewItem;
                var newValue = _selector(newKey);
                _snapshot.Add(newKey, newValue);
                Updated?.Invoke(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Insert, newKey, newValue));
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var removedKey = args.OldItem;
                var removedValue = _snapshot[removedKey];
                _snapshot.Remove(removedKey);
                Updated?.Invoke(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Remove, removedKey, default(TValue), removedValue));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                // ??
            }
        }

        public IReadOnlyDictionary<TKey, TValue> Snapshot => _snapshot;

        public event DictionaryUpdateHandler<TKey, TValue> Updated;

        protected override void DoDispose()
        {
            _srcSet.Updated -= SrcSet_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
            Updated?.Invoke(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            _srcSet.Dispose();
        }
    }
}
