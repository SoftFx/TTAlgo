using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    class SetTransformOperator<TIn, TOut> : OperatorBase, IDynamicSetSource<TOut>
    {
        private Dictionary<TIn, TOut> _snapshot = new Dictionary<TIn, TOut>();
        private IDynamicSetSource<TIn> _srcSet;
        private Func<TIn, TOut> _selector;

        public SetTransformOperator(IDynamicSetSource<TIn> srcSet, Func<TIn, TOut> selector)
        {
            _srcSet = srcSet;
            _selector = selector;

            foreach (var key in srcSet.Snapshot)
                _snapshot.Add(key, selector(key));

            srcSet.Updated += SrcSet_Updated;
        }

        private void SrcSet_Updated(SetUpdateArgs<TIn> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
            {
                var srcItem = args.NewItem;
                var resultItem = _selector(srcItem);
                _snapshot.Add(srcItem, resultItem);
                Updated?.Invoke(new SetUpdateArgs<TOut>(this, DLinqAction.Insert, resultItem));
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var srcItem = args.OldItem;
                var resultItem = _snapshot[srcItem];
                _snapshot.Remove(srcItem);
                Updated?.Invoke(new SetUpdateArgs<TOut>(this, DLinqAction.Remove, default(TOut), resultItem));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                // ??
            }
        }

        public IEnumerable<TOut> Snapshot => _snapshot.Values;

        public event SetUpdateHandler<TOut> Updated;

        protected override void DoDispose()
        {
            _srcSet.Updated -= SrcSet_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
            Updated?.Invoke(new SetUpdateArgs<TOut>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            _srcSet.Dispose();
        }
    }
}
