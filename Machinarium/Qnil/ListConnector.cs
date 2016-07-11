using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class ListConnector<T> : OperatorBase
    {
        private IDynamicListSource<T> src;
        private IList target;

        public ListConnector(IDynamicListSource<T> src, IList target)
        {
            this.src = src;
            this.target = target;

            foreach (T item in src.Snapshot)
                target.Add(item);

            src.Updated += Src_Updated;
        }

        private void Src_Updated(ListUpdateArgs<T> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
                target.Insert(args.Index, args.NewItem);
            else if (args.Action == DLinqAction.Remove)
                target.RemoveAt(args.Index);
            else if (args.Action == DLinqAction.Replace)
                target[args.Index] = args.NewItem;
        }

        protected override void DoDispose()
        {
            this.src.Updated -= Src_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
        }

        protected override void SendDisposeToSources()
        {
            src.Dispose();
        }
    }
}
