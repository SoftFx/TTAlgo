using Machinarium.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class DictionarySorter<TKey, TValue, TBy> : OperatorBase, IDynamicListSource<TValue>, IReadOnlyList<TValue>
    {
        private IDynamicDictionarySource<TKey, TValue> src;
        private SortedList<Item> list;
        private Func<TKey, TValue, TBy> selector;

        public DictionarySorter(IDynamicDictionarySource<TKey, TValue> src, Func<TKey, TValue, TBy> bySelector, IComparer<TBy> comparer)
        {
            this.src = src;
            this.selector = bySelector;

            list = new SortedList<Item>(new LocalComparer(comparer));

            foreach (var item in src.Snapshot)
            {
                var byProperty = bySelector(item.Key, item.Value);
                list.Add(new Item(item.Key, item.Value, byProperty));
            }

            src.Updated += Src_Updated;
        }

        public IReadOnlyList<TValue> Snapshot { get { return this; } }
        int IReadOnlyCollection<TValue>.Count { get { return list.Count; } }
        TValue IReadOnlyList<TValue>.this[int index] { get { return list[index].value; } }

        public event ListUpdateHandler<TValue> Updated;

        private Item CreateItem(TKey key, TValue value)
        {
            var orderProp = selector(key, value);
            return new Item(key, value, orderProp);
        }

        private void OnUpdated(ListUpdateArgs<TValue> args)
        {
            if (Updated != null)
                Updated(args);
        }

        private void Add(TKey key, TValue val)
        {
            var item = CreateItem(key, val);
            var index = list.Add(item);
            OnUpdated(new ListUpdateArgs<TValue>(this, DLinqAction.Insert, index, val));
        }

        private void Remove(TKey key, TValue val)
        {
            var item = CreateItem(key, val);
            var index = list.Remove(item);
            if (index < 0)
                throw new Exception("Cannot find removed item in list!");
            OnUpdated(new ListUpdateArgs<TValue>(this, DLinqAction.Remove, index, default(TValue), val));
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
            {
                Remove(args.Key, args.OldItem);
                Add(args.Key, args.NewItem);
            }
        }

        protected override void DoDispose()
        {
            src.Updated += Src_Updated;
        }

        protected override void SendDisposeToConsumers()
        {
            OnUpdated(new ListUpdateArgs<TValue>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            src.Dispose();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            foreach (var item in list)
                yield return item.value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in list)
                yield return item.value;
        }

        private class LocalComparer : IComparer<Item>
        {
            private static readonly Comparer<TKey> keyComparer = Comparer<TKey>.Default;
            private IComparer<TBy> byComparer;

            public LocalComparer(IComparer<TBy> byComparer)
            {
                this.byComparer = byComparer;
            }

            public int Compare(Item x, Item y)
            {
                var resultByProp = byComparer.Compare(x.sortProperty, y.sortProperty);
                if (resultByProp != 0)
                    return resultByProp;
                return keyComparer.Compare(x.key, y.key);
            }
        }

        private struct Item
        {
            public TKey key;
            public TBy sortProperty;
            public TValue value;

            public Item(TKey key, TValue value, TBy sortProperty)
            {
                this.key = key;
                this.value = value;
                this.sortProperty = sortProperty;
            }
        }
    }
}
