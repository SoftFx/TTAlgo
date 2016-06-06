using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class ListComposition<TSource, TResult> : IDynamicListSource<TResult>, IReadOnlyList<TResult>
    {
        private IDynamicListSource<TSource> src;
        private List<Observer> observers = new List<Observer>();
        private Func<TSource, IDynamicListSource<TResult>> selector;
        private bool propogateDispose;

        public ListComposition(
            IDynamicListSource<TSource> src,
            Func<TSource, IDynamicListSource<TResult>> selector,
            bool propogateDispose)
        {
            this.src = src;
            this.selector = selector;
            this.propogateDispose = propogateDispose;

            foreach (var srcElement in src.Snapshot)
                AddObserver(srcElement);

            src.Updated += CollectionList_Updated;
        }

        public int Count { get; private set; }
        public IReadOnlyList<TResult> Snapshot { get { return this; } }
        public TResult this[int index]
        {
            get
            {
                int collectionIndex;
                var localIndex = ToLocalIndex(index, out collectionIndex);
                return observers[collectionIndex].Collection.Snapshot[localIndex];
            }
        }
        public IReadOnlyList<TSource> SourceCollections { get { return src.Snapshot; } }

        public event ListUpdateHandler<TResult> Updated;

        public IEnumerator<TResult> GetEnumerator()
        {
            foreach (var observer in observers)
            {
                var snapshot = observer.Collection.Snapshot;
                foreach (var element in snapshot)
                    yield return element;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            src.Updated -= CollectionList_Updated;
            foreach (var observer in observers)
                observer.Dispose();
            OnUpdated(new ListUpdateArgs<TResult>(this, DLinqAction.Dispose));
            if (propogateDispose)
                src.Dispose();
        }

        private void OnUpdated(ListUpdateArgs<TResult> args)
        {
            if (Updated != null)
                Updated(args);
        }

        private void AddObserver(TSource srcElement)
        {
            var observer = new Observer(selector(srcElement), this);
            observers.Add(observer);
            observer.FireAllAdded();
        }

        private void CollectionList_Updated(ListUpdateArgs<TSource> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
                AddObserver(args.NewItem);
            else if (args.Action == DLinqAction.Remove)
            {
                var observer = observers[args.Index];
                var globalStartIndex = GetCollectionStartPosition(observer);
                observers.RemoveAt(args.Index);
                observer.FireAllRemoved(globalStartIndex);
                observer.Dispose();
            }
            else if (args.Action == DLinqAction.Replace)
            {
                var observer = observers[args.Index];
                var globalStartIndex = GetCollectionStartPosition(observer);
                observers.RemoveAt(args.Index);
                observer.FireAllRemoved(globalStartIndex);
                observer.Dispose();

                observer = new Observer(selector(args.NewItem), this);
                observers.Insert(args.Index, observer);
                observer.FireAllAdded();
            }
        }

        private int ToGlobalIndex(Observer colletionObserver, int localIndex)
        {
            return GetCollectionStartPosition(colletionObserver) + localIndex;
        }

        private int ToLocalIndex(int globalIndex, out int collectionIndex)
        {
            int start = 0;

            for (int i = 0; i < src.Snapshot.Count; i++)
            {
                var collection = selector(src.Snapshot[i]);
                var end = start + collection.Snapshot.Count;

                if (globalIndex >= start && globalIndex < end)
                {
                    collectionIndex = i;
                    return globalIndex - start;
                }
            }

            throw new Exception("Global index out of boundaries! Possible concurrency problems!");
        }

        private int GetCollectionStartPosition(Observer observer)
        {
            int start = 0;

            for (int i = 0; i < observers.Count; i++)
            {
                if (observers[i] == observer)
                    return start;

                start += observers[i].Collection.Snapshot.Count;
            }

            throw new Exception("Collection index out of boundaries! Possible concurrency problems!");
        }

        private class Observer
        {
            private ListComposition<TSource, TResult> parent;

            public Observer(IDynamicListSource<TResult> collection, ListComposition<TSource, TResult> parent)
            {
                this.Collection = collection;
                this.parent = parent;
                collection.Updated += Collection_Updated;
            }

            public IDynamicListSource<TResult> Collection { get; private set; }

            public void FireAllRemoved(int position)
            {
                for (int i = 0; i < Collection.Snapshot.Count; i++)
                {
                    var removedItem = Collection.Snapshot[i];
                    parent.Count--;
                    parent.OnUpdated(new ListUpdateArgs<TResult>(parent, DLinqAction.Remove, position + i, default(TResult), removedItem));
                }
            }

            public void FireAllAdded()
            {
                var position = parent.GetCollectionStartPosition(this);
                for (int i = 0; i < Collection.Snapshot.Count; i++)
                {
                    var newItem = Collection.Snapshot[i];
                    parent.Count++;
                    parent.OnUpdated(new ListUpdateArgs<TResult>(parent, DLinqAction.Insert, position + i, newItem));
                }
            }

            private void Collection_Updated(ListUpdateArgs<TResult> args)
            {
                if (args.Action == DLinqAction.Dispose)
                    Dispose();
                else if (args.Action == DLinqAction.Insert)
                {
                    var globalIndex = parent.ToGlobalIndex(this, args.Index);
                    parent.Count++;
                    parent.OnUpdated(new ListUpdateArgs<TResult>(parent, DLinqAction.Insert, globalIndex, args.NewItem));
                }
                else if (args.Action == DLinqAction.Remove)
                {
                    var globalIndex = parent.ToGlobalIndex(this, args.Index);
                    parent.Count--;
                    parent.OnUpdated(new ListUpdateArgs<TResult>(parent, DLinqAction.Remove, globalIndex, default(TResult), args.OldItem));
                }
                else if (args.Action == DLinqAction.Replace)
                {
                    var globalIndex = parent.ToGlobalIndex(this, args.Index);
                    parent.OnUpdated(new ListUpdateArgs<TResult>(parent, DLinqAction.Replace, globalIndex, args.NewItem, args.OldItem));
                }
            }

            public void Dispose()
            {
                Collection.Updated -= Collection_Updated;
            }
        }
    }
}
