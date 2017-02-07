using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class ListCopy<T> : OperatorBase, IDynamicListSource<T>, IReadOnlyList<T>
    {
        private IDynamicListSource<T> src;
        private List<T> innerList;
        private bool propogateDispose;

        public ListCopy(IDynamicListSource<T> src, bool propogateDispose)
        {
            this.src = src;
            this.innerList = new List<T>(src.Snapshot);
            this.propogateDispose = propogateDispose;

            src.Updated += Src_Updated;
        }

        public int Count { get { return src.Snapshot.Count; } }
        public IReadOnlyList<T> Snapshot { get { return this; } }
        public T this[int index] { get { return innerList[index]; } }

        public event ListUpdateHandler<T> Updated;

        protected override void SendDisposeToConsumers()
        {
            OnUpdated(new ListUpdateArgs<T>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            if (propogateDispose)
                src.Dispose();
        }

        protected override void DoDispose()
        {
            src.Updated -= Src_Updated;
        }

        private void Src_Updated(ListUpdateArgs<T> args)
        {
            if (args.Action == DLinqAction.Dispose)
            {
                Dispose();
                return;
            }
            else if (args.Action == DLinqAction.Insert)
                innerList.Insert(args.Index, args.NewItem);
            else if (args.Action == DLinqAction.Remove)
                innerList.RemoveAt(args.Index);
            else if (args.Action == DLinqAction.Replace)
                innerList[args.Index] = args.NewItem;

            OnUpdated(new ListUpdateArgs<T>(this, args.Action, args.Index, args.NewItem, args.OldItem));
        }

        private void OnUpdated(ListUpdateArgs<T> args)
        {
            if (Updated != null)
                Updated(args);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
    }
}
