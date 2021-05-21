using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class DictionaryComposition<TKey, TValue> : OperatorBase, IVarSet<TKey, TValue>
    {
        private Dictionary<TKey, TValue> snapshot = new Dictionary<TKey, TValue>();
        private CompositionSource src;

        public IReadOnlyDictionary<TKey, TValue> Snapshot { get { return snapshot; } }

        public event DictionaryUpdateHandler<TKey, TValue> Updated;

        public DictionaryComposition(CompositionSource src)
        {
            this.src = src;

            src.Init(this);
            src.OnStart();
        }

        private void Add(IVarSet<TKey, TValue> src)
        {
            foreach (var keyValue in src.Snapshot)
            {
                snapshot.Add(keyValue.Key, keyValue.Value);
                OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Insert, keyValue.Key, keyValue.Value));
            }

            src.Updated += Src_Updated;
        }

        private void Remove(IVarSet<TKey, TValue> src)
        {
            foreach (var keyValue in src.Snapshot)
            {
                snapshot.Remove(keyValue.Key);
                OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Remove, keyValue.Key, default(TValue), keyValue.Value));
            }

            src.Updated -= Src_Updated;
        }

        private void AddElement(TKey key, TValue val)
        {
            snapshot.Add(key, val);
            OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Insert, key, val));
        }

        private void RemoveElement(TKey key, TValue val)
        {
            snapshot.Remove(key);
            OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Remove, key, default(TValue), val));
        }

        private void ReplaceElement(TKey key, TValue newVal, TValue oldVal)
        {
            snapshot[key] = newVal;
            OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Replace, key, newVal, oldVal));
        }

        private void Src_Updated(DictionaryUpdateArgs<TKey, TValue> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
                AddElement(args.Key, args.NewItem);
            else if (args.Action == DLinqAction.Remove)
                RemoveElement(args.Key, args.OldItem);
            else if (args.Action == DLinqAction.Replace)
                ReplaceElement(args.Key, args.NewItem, args.OldItem);
        }

        private void OnUpdated(DictionaryUpdateArgs<TKey, TValue> args)
        {
            Updated?.Invoke(args);
        }

        protected override void DoDispose()
        {
            src.OnStop();
        }

        protected override void SendDisposeToConsumers()
        {
            OnUpdated(new DictionaryUpdateArgs<TKey, TValue>(this, DLinqAction.Dispose));
        }

        protected override void SendDisposeToSources()
        {
            src.DisposeSources();
        }

        public abstract class CompositionSource
        {
            public void Init(DictionaryComposition<TKey, TValue> parent)
            {
                this.Composition = parent;
            }

            protected DictionaryComposition<TKey, TValue> Composition { get; private set; }

            public abstract void OnStart();
            public abstract void OnStop();
            public abstract void DisposeSources();
        }

        public class StaticCompositionSource<TSource> : CompositionSource
        {
            private IEnumerable<TSource> src;
            private Func<TSource, IVarSet<TKey, TValue>> selector;

            public StaticCompositionSource(IEnumerable<TSource> src,
                Func<TSource, IVarSet<TKey, TValue>> selector)
            {
                this.src = src;
                this.selector = selector;
            }

            public override void OnStart()
            {
                foreach (var element in src)
                    Composition.Add(selector(element));
            }

            public override void OnStop()
            {
            }

            public override void DisposeSources()
            {
            }
        }

        public class DictionaryCompositionSource<TSourceKey, TSourceValue> : CompositionSource
        {
            private IVarSet<TSourceKey, TSourceValue> src;
            private Func<TSourceKey, TSourceValue, IVarSet<TKey, TValue>> selector;

            public DictionaryCompositionSource(IVarSet<TSourceKey, TSourceValue> src,
                Func<TSourceKey, TSourceValue, IVarSet<TKey, TValue>> selector)
            {
                this.src = src;
                this.selector = selector;
            }

            public override void OnStart()
            {
                foreach (var pair in src.Snapshot)
                    Composition.Add(selector(pair.Key, pair.Value));

                src.Updated += Src_Updated;
            }

            public override void OnStop()
            {
            }

            public override void DisposeSources()
            {
                src.Dispose();
            }

            private void Src_Updated(DictionaryUpdateArgs<TSourceKey, TSourceValue> args)
            {
            }
        }

        public class ListCompositionSource<TSource> : CompositionSource
        {
            private IVarList<TSource> src;
            private List<IVarSet<TKey, TValue>> cache = new List<IVarSet<TKey, TValue>>();
            private Func<TSource, IVarSet<TKey, TValue>> selector;

            public ListCompositionSource(IVarList<TSource> src,
                Func<TSource, IVarSet<TKey, TValue>> selector)
            {
                this.src = src;
                this.selector = selector;
            }

            public override void OnStart()
            {
                foreach (var element in src.Snapshot)
                    Insert(cache.Count, element);

                src.Updated += Src_Updated;
            }

            public override void OnStop()
            {
                src.Updated -= Src_Updated;
            }

            public override void DisposeSources()
            {
                src.Dispose();
            }

            private void Insert(int index, TSource srcElement)
            {
                var collection = selector(srcElement);
                cache.Insert(index, collection);
                Composition.Add(collection);
            }

            private void Remove(int index)
            {
                var collection = cache[index];
                cache.RemoveAt(index);
                Composition.Remove(collection);
            }

            private void Src_Updated(ListUpdateArgs<TSource> args)
            {
                if (args.Action == DLinqAction.Dispose)
                    Composition.Dispose();
                else if (args.Action == DLinqAction.Insert)
                    Insert(args.Index, args.NewItem);
                else if (args.Action == DLinqAction.Remove)
                    Remove(args.Index);
                else if (args.Action == DLinqAction.Replace)
                {
                    Remove(args.Index);
                    Insert(args.Index, args.NewItem);
                }
            }
        }
    }
}
