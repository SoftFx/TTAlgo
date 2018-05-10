using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    class SetSelectOperator<TIn, TOut> : OperatorBase, IVarSet<TOut>
    {
        private IVarSet<TIn> _srcSet;
        private Func<TIn, TOut> _selector;

        public SetSelectOperator(IVarSet<TIn> srcSet, Func<TIn, TOut> selector)
        {
            _srcSet = srcSet;
            _selector = selector;

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
                Updated?.Invoke(new SetUpdateArgs<TOut>(this, DLinqAction.Insert, resultItem));
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var srcItem = args.OldItem;
                var resultItem = _selector(srcItem);
                Updated?.Invoke(new SetUpdateArgs<TOut>(this, DLinqAction.Remove, default(TOut), resultItem));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                // ??
            }
        }

        public IEnumerable<TOut> Snapshot
        {
            get
            {
                foreach (var srcEntry in _srcSet.Snapshot)
                    yield return _selector(srcEntry);
            }
        }

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
