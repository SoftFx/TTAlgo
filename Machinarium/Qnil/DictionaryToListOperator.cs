using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class DictionaryToListOperator<TKey, TValue, TResult> : OperatorBase, IDynamicListSource<TResult>, IReadOnlyList<TResult>
    {
        private static readonly IEqualityComparer<TKey> Comparer = EqualityComparer<TKey>.Default;

        private IDynamicDictionarySource<TKey, TValue> _src;
        private List<ListItem> _list;
        private Func<TKey, TValue, TResult> _selector;

        public DictionaryToListOperator(IDynamicDictionarySource<TKey, TValue> src, Func<TKey, TValue, TResult> selector)
        {
            _src = src;
            _selector = selector;

            _list = new List<ListItem>();

            foreach (var srcItem in src.Snapshot)
                _list.Add(new ListItem(srcItem.Key, _selector(srcItem.Key, srcItem.Value)));

            src.Updated += Src_Updated;
        }

        public IReadOnlyList<TResult> Snapshot { get { return this; } }

        public event ListUpdateHandler<TResult> Updated;

        private void Add(TKey key, TValue val)
        {
            var newVal = _selector(key, val);
            var index = _list.Count;
            _list.Add(new ListItem(key, newVal));
            Updated?.Invoke(new ListUpdateArgs<TResult>(this, DLinqAction.Insert, index, newVal));
        }

        private void Remove(TKey key, TValue val)
        {
            var index = _list.FindIndex(0, i => Comparer.Equals(i.Key, key));
            if (index < 0)
                throw new Exception("Cannot find removed item in list!");
            var removedVal = _list[index].Val;
            _list.RemoveAt(index);
            Updated?.Invoke(new ListUpdateArgs<TResult>(this, DLinqAction.Remove, index, default(TResult), removedVal));
        }

        private void Replace(TKey key, TValue val)
        {
            var index = _list.FindIndex(0, i => Comparer.Equals(i.Key, key));
            if (index < 0)
                throw new Exception("Cannot find removed item in list!");
            var item = _list[index];
            var oldVal = item.Val;
            var newVal = _selector(key, val);
            _list[index] = new ListItem(key, newVal);
            Updated?.Invoke(new ListUpdateArgs<TResult>(this, DLinqAction.Replace, index, default(TResult), newVal));
        }

        private void Src_Updated(DictionaryUpdateArgs<TKey, TValue> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
                Add(args.Key, args.NewItem);
            else if (args.Action == DLinqAction.Remove)
                Remove(args.Key, args.OldItem);
            else if (args.Action == DLinqAction.Replace)
                Replace(args.Key, args.NewItem);
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
            public ListItem(TKey key, TResult value)
            {
                Key = key;
                Val = value;
            }

            public TKey Key { get; set; }
            public TResult Val { get; set; }
        }
    }
}
