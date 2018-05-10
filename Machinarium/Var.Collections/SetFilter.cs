using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class SetFilter<T> : OperatorBase, IVarSet<T>, IEnumerable<T>
    {
        private IVarSet<T> src;
        private Predicate<T> condition;

        public SetFilter(IVarSet<T> src, Predicate<T> condition)
        {
            this.src = src;
            this.condition = condition;

            src.Updated += Src_Updated;
        }

        public IEnumerable<T> Snapshot => this;
        public event SetUpdateHandler<T> Updated;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var i in src.Snapshot)
            {
                if (condition(i))
                    yield return i;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void Src_Updated(SetUpdateArgs<T> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
            {
                if (condition(args.NewItem))
                    Updated?.Invoke(args);
            }
            else if (args.Action == DLinqAction.Remove)
            {
                if (condition(args.OldItem))
                    Updated?.Invoke(args);
            }
        }

        protected override void DoDispose()
        {
            src.Updated -= Src_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
            Updated?.Invoke(new SetUpdateArgs<T>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            src.Dispose();
        }
    }
}
