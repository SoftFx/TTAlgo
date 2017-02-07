using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class ListAdapter<T> : OperatorBase, IDynamicListSource<T>
    {
        private IReadOnlyList<T> list;

        public ListAdapter(IReadOnlyList<T> list)
        {
            this.list = list;
        }

        public IReadOnlyList<T> Snapshot { get { return list; } }

        public event ListUpdateHandler<T> Updated;

        protected override void DoDispose()
        {
        }

        protected override void SendDisposeToConsumers()
        {
            if (Updated != null)
                Updated(new ListUpdateArgs<T>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
        }
    }
}
