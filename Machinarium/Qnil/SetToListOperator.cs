using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class SetToListOperator<TSource, TResult> : OperatorBase, IReadOnlyList<TResult>, IVarList<TResult>
    {
        private static readonly IEqualityComparer<TSource> Comparer = EqualityComparer<TSource>.Default;

        private IVarSet<TSource> _src;
        private List<ListItem> _list;
        private Func<TSource, TResult> _selector;

        public SetToListOperator(IVarSet<TSource> src, Func<TSource, TResult> selector)
        {
            _src = src;
            _selector = selector;

            _list = new List<ListItem>();

            foreach (var srcItem in src.Snapshot)
                _list.Add(new ListItem(srcItem, _selector(srcItem)));

            src.Updated += Src_Updated;
        }

        public IReadOnlyList<TResult> Snapshot { get { return this; } }

        public event ListUpdateHandler<TResult> Updated;

        private void Add(TSource srcValue)
        {
            var newVal = _selector(srcValue);
            var index = _list.Count;
            _list.Add(new ListItem(srcValue, newVal));
            Updated?.Invoke(new ListUpdateArgs<TResult>(this, DLinqAction.Insert, index, newVal));
        }

        private void Remove(TSource srcValue)
        {
            var index = _list.FindIndex(0, i => Comparer.Equals(i.Key, srcValue));
            if (index < 0)
                throw new Exception("Cannot find removed item in list!");
            var removedVal = _list[index].Val;
            _list.RemoveAt(index);
            Updated?.Invoke(new ListUpdateArgs<TResult>(this, DLinqAction.Remove, index, default(TResult), removedVal));
        }

        private void Replace(TSource oldSrcVal, TSource newSrcVal)
        {
            var index = _list.FindIndex(0, i => Comparer.Equals(i.Key, oldSrcVal));
            if (index < 0)
                throw new Exception("Cannot find removed item in list!");
            var item = _list[index];
            var oldVal = item.Val;
            var newVal = _selector(newSrcVal);
            _list[index] = new ListItem(newSrcVal, newVal);
            Updated?.Invoke(new ListUpdateArgs<TResult>(this, DLinqAction.Replace, index, default(TResult), newVal));
        }

        private void Src_Updated(SetUpdateArgs<TSource> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
                Add(args.NewItem);
            else if (args.Action == DLinqAction.Remove)
                Remove(args.OldItem);
            else if (args.Action == DLinqAction.Replace)
                Replace(args.OldItem, args.NewItem);
        }

        protected override void DoDispose()
        {
            _src.Updated += Src_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
            Updated?.Invoke(new ListUpdateArgs<TResult>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            _src.Dispose();
        }

        #region IReadOnlyList<TResult>

        public int Count => _list.Count;

        public TResult this[int index] => _list[index].Val;

        public IEnumerator<TResult> GetEnumerator()
        {
            foreach (var i in _list)
                yield return i.Val;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var i in _list)
                yield return i.Val;
        }

        #endregion

        private struct ListItem
        {
            public ListItem(TSource key, TResult value)
            {
                Key = key;
                Val = value;
            }

            public TSource Key { get; set; }
            public TResult Val { get; set; }
        }
    }
}
