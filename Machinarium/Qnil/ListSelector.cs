using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class ListSelector<TSource, TResult> : IVarList<TResult>, IReadOnlyList<TResult>
    {
        private IVarList<TSource> src;
        private List<TResult> innerList = new List<TResult>();
        private Func<TSource, TResult> selectFunc;
        private bool propogateDispose;

        private static string prefix = "ListSelector<" + typeof(TResult).Name + "> ";

        public ListSelector(IVarList<TSource> src, Func<TSource, TResult> selectFunc, bool propogateDispose)
        {
            this.src = src;
            this.selectFunc = selectFunc;
            this.propogateDispose = propogateDispose;

            //System.Diagnostics.Debug.WriteLine(prefix + " .CTOR");

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
            Updated?.Invoke(args);
        }

        private void Add(TSource srcItem)
        {
            //System.Diagnostics.Debug.WriteLine(prefix + " ADD");

            var newItem = selectFunc(srcItem);
            innerList.Add(newItem);
            OnUpdated(new ListUpdateArgs<TResult>(this, DLinqAction.Insert, innerList.Count - 1, newItem));
        }

        private void Src_Updated(ListUpdateArgs<TSource> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
            {
                //System.Diagnostics.Debug.WriteLine(prefix + " INSERT");

                var newItem = selectFunc(args.NewItem);
                innerList.Insert(args.Index, newItem);
                OnUpdated(new ListUpdateArgs<TResult>(this, args.Action, args.Index, newItem));
            }
            else if (args.Action == DLinqAction.Remove)
            {
                //System.Diagnostics.Debug.WriteLine(prefix + " REMOVE");

                var removedItem = innerList[args.Index];
                innerList.RemoveAt(args.Index);
                OnUpdated(new ListUpdateArgs<TResult>(this, args.Action, args.Index, default(TResult), removedItem));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                //System.Diagnostics.Debug.WriteLine(prefix + " REPLACE");

                var oldItem = innerList[args.Index];
                var newItem = selectFunc(args.NewItem);
                innerList[args.Index] = newItem;
                OnUpdated(new ListUpdateArgs<TResult>(this, args.Action, args.Index, newItem, oldItem));
            }
        }
    }
}
