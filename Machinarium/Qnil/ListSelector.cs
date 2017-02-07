using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class ListSelector<TSource, TResult> : IDynamicListSource<TResult>, IReadOnlyList<TResult>
    {
        private IDynamicListSource<TSource> src;
        private List<TResult> innerList = new List<TResult>();
        private Func<TSource, TResult> selectFunc;
        private bool propogateDispose;

        public ListSelector(IDynamicListSource<TSource> src, Func<TSource, TResult> selectFunc, bool propogateDispose)
        {
            this.src = src;
            this.selectFunc = selectFunc;
            this.propogateDispose = propogateDispose;

            foreach (var srcItem in src.Snapshot)
                Add(srcItem);

            src.Updated += Src_Updated;
        }

        public int Count { get { return src.Snapshot.Count; } }
        public IReadOnlyList<TResult> Snapshot { get { return this; } }
        public TResult this[int index] { get { return selectFunc(src.Snapshot[index]); } }

        public event ListUpdateHandler<TResult> Updated;

        public IEnumerator<TResult> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            src.Updated -= Src_Updated;
            OnUpdated(new ListUpdateArgs<TResult>(this, DLinqAction.Dispose));
            if (propogateDispose)
                src.Dispose();
        }

        private void OnUpdated(ListUpdateArgs<TResult> args)
        {
            if (Updated != null)
                Updated(args);
        }

        private void Add(TSource srcItem)
        {
            var newItem = selectFunc(srcItem);
            OnUpdated(new ListUpdateArgs<TResult>(this, DLinqAction.Insert, innerList.Count - 1, newItem));
        }

        private void Src_Updated(ListUpdateArgs<TSource> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
            {
                var newItem = selectFunc(args.NewItem);
                innerList.Insert(args.Index, newItem);
                OnUpdated(new ListUpdateArgs<TResult>(this, args.Action, args.Index, newItem));
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var removedItem = innerList[args.Index];
                innerList.RemoveAt(args.Index);
                OnUpdated(new ListUpdateArgs<TResult>(this, args.Action, args.Index, default(TResult), removedItem));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                var oldItem = innerList[args.Index];
                var newItem = selectFunc(args.NewItem);
                innerList[args.Index] = newItem;
                OnUpdated(new ListUpdateArgs<TResult>(this, args.Action, args.Index, newItem, oldItem));
            }
        }
    }
}
