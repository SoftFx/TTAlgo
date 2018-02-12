using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class SetUnionOperator<T> : OperatorBase, IDynamicSetSource<T>
    {
        private Dictionary<T, Counter> _countByItem = new Dictionary<T, Counter>();
        private IDynamicSetSource<T>[] _sources;

        public SetUnionOperator(IDynamicSetSource<T>[] sources)
        {
            _sources = sources;

            foreach (var src in _sources)
            {
                foreach (var i in src.Snapshot)
                    Add(i);

                src.Updated += Src_Updated;
            }
        }

        public event SetUpdateHandler<T> Updated;
        public IEnumerable<T> Snapshot => _countByItem.Keys;

        protected override void DoDispose()
        {
            foreach (var src in _sources)
                src.Updated -= Src_Updated;
        }

        private void Add(T item)
        {
            Counter counter;
            if (!_countByItem.TryGetValue(item, out counter))
            {
                counter = new Counter();
                _countByItem.Add(item, counter);
            }
            counter.Increase();
            if (counter.Count == 1)
                Updated?.Invoke(new SetUpdateArgs<T>(this, DLinqAction.Insert, item));
        }

        private void Remove(T item)
        {
            var itemCounter = _countByItem[item];
            itemCounter.Decrease();
            if (itemCounter.Count == 0)
            {
                _countByItem.Remove(item);
                Updated?.Invoke(new SetUpdateArgs<T>(this, DLinqAction.Remove, item));
            }
        }

        protected override void SendDisposeToConsumers()
        {
            Updated?.Invoke(new SetUpdateArgs<T>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            foreach (var src in _sources)
                src.Dispose();
        }

        private void Src_Updated(SetUpdateArgs<T> args)
        {
            if (args.Action == DLinqAction.Insert)
                Add(args.NewItem);
            else if (args.Action == DLinqAction.Remove)
                Remove(args.OldItem);
        }

        private class Counter
        {
            public int Count { get; private set; }
            public void Increase() { Count++; }
            public void Decrease() { Count--; }
        }
    }
}
