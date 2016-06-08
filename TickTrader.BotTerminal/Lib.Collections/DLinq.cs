using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public static class DLinq
    {
        public static IDynamicListSource<TResult> Select<TSource, TResult>(this IDynamicListSource<TSource> src, Func<TSource, TResult> selector)
        {
            var chain = src as ChainProxy<TSource>;

            if (chain != null)
                return new ListSelector<TSource, TResult>(chain.Src, selector, true);

            return new ListSelector<TSource, TResult>(src, selector, false);
        }

        public static IDynamicListSource<T> Where<T>(this IDynamicListSource<T> src, Predicate<T> condition)
        {
            var chain = src as ChainProxy<T>;

            if (chain != null)
                return new ListFilter<T>(chain.Src, condition, true);

            return new ListFilter<T>(src, condition, false);
        }

        public static IDynamicListSource<T> ToList<T>(this IDynamicListSource<T> src)
        {
            var chain = src as ChainProxy<T>;

            if (chain != null)
                return new ProjectionList<T>(chain.Src, true);

            return new ProjectionList<T>(src, false);
        }

        public static IObservableListSource<T> AsObservable<T>(this IDynamicListSource<T> src)
        {
            var chain = src as ChainProxy<T>;

            if (chain != null)
                return new ObservableWrapper<T>(chain.Src, true);

            return new ObservableWrapper<T>(src, false);
        }

        /// <summary>
        /// Binds operators in chain. Disposing of any operator in the chain will cause dispose of entire chain.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static IDynamicListSource<T> Chain<T>(this IDynamicListSource<T> src)
        {
            return new ChainProxy<T>(src);
        }

        public static IDynamicListSource<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> src,
            Func<TSource, IDynamicListSource<TResult>> selector)
        {
            return new StaticCompositionOfDynamicLists<TResult>(src.Select(selector));
        }

        public static IDynamicListSource<T> Combine<T>(params IDynamicListSource<T>[] collections)
        {
            return new StaticCompositionOfDynamicLists<T>(collections);
        }

        public static IDynamicListSource<T> CombineChained<T>(params DynamicList<T>[] collections)
        {
            return new StaticCompositionOfDynamicLists<T>(collections) { PropogateDispose = true };
        }

        public static IDynamicListSource<TResult> SelectMany<TSource, TResult>(this IDynamicListSource<TSource> src,
            Func<TSource, IDynamicListSource<TResult>> selector)
        {
            var chain = src as ChainProxy<TSource>;

            if (chain != null)
                return new DynamicCompositionOfDynamicLists<TSource, TResult>(chain.Src, selector, true);

            return new DynamicCompositionOfDynamicLists<TSource, TResult>(src, selector, false);
        }

        public static IDynamicListSource<TResult> SelectMany<TSource, TResult>(this IDynamicListSource<TSource> src,
            Func<TSource, IReadOnlyList<TResult>> selector)
        {
            var chain = src as ChainProxy<TSource>;

            if (chain != null)
                return new DynamicCompositionOfEnumerables<TSource, TResult>(chain.Src, selector, true);

            return new DynamicCompositionOfEnumerables<TSource, TResult>(src, selector, false);
        }

        /// <summary>
        /// Note: Supplied collection must be empty!
        /// Note: You should not update supplied collection in any other way or from any other source!
        /// </summary>
        public static IDisposable ConnectTo<T>(this IDynamicListSource<T> src, IList target)
        {
            var chain = src as ChainProxy<T>;

            if (chain != null)
                return new CopyProxy<T>(chain.Src, target) { PropogateDispose = true };

            return new CopyProxy<T>(src, target);
        }

#if DEBUG
        public static void Test()
        {
            DynamicList<int> list1 = new DynamicList<int>();
            list1.Add(11);
            list1.Add(18);

            var less15 = list1.Where(i => i < 15);

            Assert(less15, new int[] { 11 });

            list1.Add(12);
            list1.Add(21);
            list1.Add(13);
            list1.Insert(2, 14);

            Assert(less15, new int[] { 11, 14, 12, 13 });

            list1[3] = 25; // replace 12

            Assert(less15, new int[] { 11, 14, 13 });

            list1[4] = 10; // replace 21

            Assert(less15, new int[] { 11, 14, 10, 13 });

            list1.Remove(11);
            list1.Remove(14);

            Assert(less15, new int[] { 10, 13 });

            list1.Insert(2, 30);
            list1.Insert(0, 31);
            list1.Insert(4, 38);

            Assert(less15, new int[] { 10, 13 });

            DynamicList<int> list2 = new DynamicList<int>();

            list2.Add(15);
            list2.Add(6);
            list2.Add(7);

            var less10 = list2.Where(i => i < 10);
            var above10 = list2.Where(i => i >= 10);

            var combine = DLinq.Combine(less10, above10);

            Assert(list2, new int[] { 15, 6, 7 });
            Assert(less10, new int[] { 6, 7 });
            Assert(above10, new int[] { 15 });
            Assert(combine, new int[] { 6, 7, 15 });

            list2.Insert(1, 10);
            list2.Insert(1, 4);

            Assert(list2, new int[] { 15, 4, 10, 6, 7 });
            Assert(less10, new int[] { 4, 6, 7 });
            Assert(above10, new int[] { 15, 10 });
            Assert(combine, new int[] { 4, 6, 7, 15, 10 });

            list2.RemoveAt(2);
            list2.RemoveAt(0);

            Assert(list2, new int[] { 4, 6, 7 });
            Assert(less10, new int[] { 4, 6, 7 });
            Assert(above10, new int[] { });
            Assert(combine, new int[] { 4, 6, 7 });
        }

        private static void Assert(IDynamicListSource<int> dList, IReadOnlyList<int> expected)
        {
            if (!SequenceEqual(dList.Snapshot, expected))
                throw new Exception("Assertion failed!");
        }

        private static bool SequenceEqual(IReadOnlyList<int> list1, IReadOnlyList<int> list2)
        {
            var enumCount1 = list1.Count();
            var enumCount2 = list2.Count();

            if (enumCount1 != enumCount2)
                return false;

            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }

            return true;
        }
#endif

        private abstract class ChainDisposeBase : IDisposable
        {
            private bool isDisposed;

            public ChainDisposeBase()
            {
            }

            protected abstract void DoDispose();
            protected abstract void SendDisposeToConsumers();
            protected abstract void SendDisposeToSources();

            public bool PropogateDispose { get; set; }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    DoDispose();

                    isDisposed = true;

                    SendDisposeToConsumers();

                    if (PropogateDispose)
                        SendDisposeToSources();        
                }
            }
        }

        private class ChainProxy<T> : IDynamicListSource<T>
        {
            private IDynamicListSource<T> src;

            public ChainProxy(IDynamicListSource<T> src)
            {
                this.src = src;
            }

            public IDynamicListSource<T> Src { get { return src; } }
            public IReadOnlyList<T> Snapshot { get { return src.Snapshot; } }
            public event ListUpdateHandler<T> Updated { add { src.Updated += value; } remove { src.Updated -= value; } }

            public void Dispose()
            {
                src.Dispose();
            }
        }

        private class ListSelector<TSource, TResult> : IDynamicListSource<TResult>, IReadOnlyList<TResult>
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

        private class ListFilter<T> : IDynamicListSource<T>, IReadOnlyList<T>
        {
            private IDynamicListSource<T> src;
            private List<int> indexMap = new List<int>();
            private Predicate<T> filterPredicate;
            private bool propogateDispose;

            public ListFilter(IDynamicListSource<T> src, Predicate<T> filterPredicate, bool propogateDispose)
            {
                this.src = src;
                this.filterPredicate = filterPredicate;
                this.propogateDispose = propogateDispose;

                for (int i = 0; i < src.Snapshot.Count; i++)
                {
                    if (filterPredicate(src.Snapshot[i]))
                        indexMap.Add(i);
                }

                src.Updated += Src_Updated;
            }

            public int Count { get { return indexMap.Count; } }
            public IReadOnlyList<T> Snapshot { get { return this; } }
            public T this[int index] { get { return src.Snapshot[indexMap[index]]; } }

            public event ListUpdateHandler<T> Updated;

            public IEnumerator<T> GetEnumerator()
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
                OnUpdated(new ListUpdateArgs<T>(this, DLinqAction.Dispose));
                if (propogateDispose)
                    src.Dispose();
            }

            private void OnUpdated(ListUpdateArgs<T> args)
            {
                if (Updated != null)
                    Updated(args);
            }

            private int FindIndexInMap(int srcIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (indexMap[i] == srcIndex)
                        return i;
                }
                return -1;
            }

            private int FindNearesTSourcedexInMap(int srcIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (indexMap[i] >= srcIndex)
                        return i;
                }
                return Count;
            }

            private void IncreaseIndexes(int startIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (indexMap[i] >= startIndex)
                        indexMap[i]++;
                }
            }

            private void DecreaseIndexes(int startIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (indexMap[i] > startIndex)
                        indexMap[i]--;
                }
            }

            private void Src_Updated(ListUpdateArgs<T> args)
            {
                if (args.Action == DLinqAction.Dispose)
                    Dispose();
                else if (args.Action == DLinqAction.Insert)
                {
                    IncreaseIndexes(args.Index);

                    if (filterPredicate(args.NewItem))
                    {
                        var newIndex = FindNearesTSourcedexInMap(args.Index);
                        indexMap.Insert(newIndex, args.Index);
                        OnUpdated(new ListUpdateArgs<T>(this, DLinqAction.Insert, newIndex, args.NewItem));
                    }
                }
                else if (args.Action == DLinqAction.Remove)
                {
                    var mapIndex = FindIndexInMap(args.Index);

                    DecreaseIndexes(args.Index);

                    if (mapIndex >= 0)
                    {
                        indexMap.RemoveAt(mapIndex);
                        OnUpdated(new ListUpdateArgs<T>(this, DLinqAction.Remove, mapIndex, default(T), args.OldItem));
                    }
                }
                else if (args.Action == DLinqAction.Replace)
                {
                    var mapIndex = FindIndexInMap(args.Index);
                    bool isFiltered = !filterPredicate(args.NewItem);

                    if (mapIndex >= 0)
                    {
                        if (!isFiltered)
                        {
                            // replace
                            indexMap[mapIndex] = args.Index;
                            OnUpdated(new ListUpdateArgs<T>(this, DLinqAction.Replace, mapIndex, args.NewItem, args.OldItem));
                        }
                        else
                        {
                            // remove
                            indexMap.RemoveAt(mapIndex);
                            OnUpdated(new ListUpdateArgs<T>(this, DLinqAction.Remove, mapIndex, default(T), args.OldItem));
                        }
                    }
                    else
                    {
                        if (!isFiltered)
                        {
                            // insert
                            var newIndex = FindNearesTSourcedexInMap(args.Index);
                            indexMap.Insert(newIndex, args.Index);
                            OnUpdated(new ListUpdateArgs<T>(this, DLinqAction.Insert, newIndex, args.NewItem));
                        }
                        else
                        {
                            // do nothing
                        }
                    }
                }
            }
        }

        private class ProjectionList<T> : IDynamicListSource<T>, IReadOnlyList<T>
        {
            private IDynamicListSource<T> src;
            private List<T> innerList;
            private bool propogateDispose;

            public ProjectionList(IDynamicListSource<T> src, bool propogateDispose)
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

            public void Dispose()
            {
                src.Updated -= Src_Updated;
                OnUpdated(new ListUpdateArgs<T>(this, DLinqAction.Dispose));
                if (propogateDispose)
                    src.Dispose();
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

        private class ObservableWrapper<T> : IObservableListSource<T>, IReadOnlyList<T>
        {
            private IDynamicListSource<T> src;
            private bool propogateDispose;
            private bool isDisposed;

            public ObservableWrapper(IDynamicListSource<T> src, bool propogateDispose)
            {
                this.src = src;
                this.propogateDispose = propogateDispose;

                src.Updated += Src_Updated;
            }

            public int Count { get { return src.Snapshot.Count; } }
            public T this[int index] { get { return src.Snapshot[index]; } }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public event PropertyChangedEventHandler PropertyChanged;

            public IEnumerator<T> GetEnumerator()
            {
                return src.Snapshot.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return src.Snapshot.GetEnumerator();
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    this.src.Updated -= Src_Updated;

                    isDisposed = true;

                    if (propogateDispose)
                        src.Dispose();
                }
            }

            private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                if (CollectionChanged != null)
                    CollectionChanged(this, e);
            }

            private void OnPropertyChanged(string name)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            private void Src_Updated(ListUpdateArgs<T> args)
            {
                if (args.Action == DLinqAction.Dispose)
                    Dispose();
                else if (args.Action == DLinqAction.Insert)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, args.NewItem, args.Index));
                    OnPropertyChanged(nameof(Count));
                }
                else if (args.Action == DLinqAction.Remove)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, args.OldItem, args.Index));
                    OnPropertyChanged(nameof(Count));
                }
                else if (args.Action == DLinqAction.Replace)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, args.NewItem, args.OldItem, args.Index));
                }
            }
        }

        private class DynamicCompositionOfDynamicLists<TSource, TResult> : IDynamicListSource<TResult>, IReadOnlyList<TResult>
        {
            private IDynamicListSource<TSource> src;
            private List<Observer> observers = new List<Observer>();
            private Func<TSource, IDynamicListSource<TResult>> selector;
            private bool propogateDispose;

            public DynamicCompositionOfDynamicLists(
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
                private DynamicCompositionOfDynamicLists<TSource, TResult> parent;

                public Observer(IDynamicListSource<TResult> collection, DynamicCompositionOfDynamicLists<TSource, TResult> parent)
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
                        parent.OnUpdated(new ListUpdateArgs<TResult>(parent, DLinqAction.Remove, position, default(TResult), removedItem));
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

        private class StaticCompositionOfDynamicLists<T> : ChainDisposeBase, IDynamicListSource<T>, IReadOnlyList<T>
        {
            private List<Observer> observers = new List<Observer>();

            public StaticCompositionOfDynamicLists(IEnumerable<IDynamicListSource<T>> src)
            {
                foreach (var collection in src)
                    AddObserver(collection);
            }

            public int Count { get; private set; }
            public IReadOnlyList<T> Snapshot { get { return this; } }

            public T this[int index]
            {
                get
                {
                    int collectionIndex;
                    var localIndex = ToLocalIndex(index, out collectionIndex);
                    return observers[collectionIndex].Collection.Snapshot[localIndex];
                }
            }

            public event ListUpdateHandler<T> Updated;

            public IEnumerator<T> GetEnumerator()
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

            protected override void SendDisposeToConsumers()
            {
                OnUpdated(new ListUpdateArgs<T>(this, DLinqAction.Dispose));
            }

            protected override void DoDispose()
            {
                foreach (var observer in observers)
                    observer.Dispose();
            }

            protected override void SendDisposeToSources()
            {
                foreach (var observer in observers)
                    observer.DisposeSource();
            }

            private void OnUpdated(ListUpdateArgs<T> args)
            {
                if (Updated != null)
                    Updated(args);
            }

            private void AddObserver(IDynamicListSource<T> collection)
            {
                var observer = new Observer(collection, this);
                observers.Add(observer);
                observer.FireAllAdded();
            }

            private int ToGlobalIndex(Observer colletionObserver, int localIndex)
            {
                return GetCollectionStartPosition(colletionObserver) + localIndex;
            }

            private int ToLocalIndex(int globalIndex, out int collectionIndex)
            {
                int start = 0;

                for (int i = 0; i < observers.Count; i++)
                {
                    var collection = observers[i].Collection;
                    var end = start + collection.Snapshot.Count;

                    if (globalIndex >= start && globalIndex < end)
                    {
                        collectionIndex = i;
                        return globalIndex - start;
                    }

                    start += collection.Snapshot.Count;
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
                private StaticCompositionOfDynamicLists<T> parent;

                public Observer(IDynamicListSource<T> collection, StaticCompositionOfDynamicLists<T> parent)
                {
                    this.Collection = collection;
                    this.parent = parent;
                    collection.Updated += Collection_Updated;
                }

                public IDynamicListSource<T> Collection { get; private set; }

                public void FireAllRemoved()
                {
                    var position = parent.GetCollectionStartPosition(this);
                    for (int i = 0; i < Collection.Snapshot.Count; i++)
                    {
                        var removedItem = Collection.Snapshot[i];
                        parent.Count--;
                        parent.OnUpdated(new ListUpdateArgs<T>(parent, DLinqAction.Remove, position + i, default(T), removedItem));
                    }
                }

                public void FireAllAdded()
                {
                    var position = parent.GetCollectionStartPosition(this);
                    for (int i = 0; i < Collection.Snapshot.Count; i++)
                    {
                        var newItem = Collection.Snapshot[i];
                        parent.Count++;
                        parent.OnUpdated(new ListUpdateArgs<T>(parent, DLinqAction.Insert, position + i, newItem));
                    }
                }

                private void Collection_Updated(ListUpdateArgs<T> args)
                {
                    if (args.Action == DLinqAction.Dispose)
                        Dispose();
                    else if (args.Action == DLinqAction.Insert)
                    {
                        var globalIndex = parent.ToGlobalIndex(this, args.Index);
                        parent.Count++;
                        parent.OnUpdated(new ListUpdateArgs<T>(parent, DLinqAction.Insert, globalIndex, args.NewItem));
                    }
                    else if (args.Action == DLinqAction.Remove)
                    {
                        var globalIndex = parent.ToGlobalIndex(this, args.Index);
                        parent.Count--;
                        parent.OnUpdated(new ListUpdateArgs<T>(parent, DLinqAction.Remove, globalIndex, default(T), args.OldItem));
                    }
                    else if (args.Action == DLinqAction.Replace)
                    {
                        var globalIndex = parent.ToGlobalIndex(this, args.Index);
                        parent.OnUpdated(new ListUpdateArgs<T>(parent, DLinqAction.Replace, globalIndex, args.NewItem, args.OldItem));
                    }
                }

                public void Dispose()
                {
                    Collection.Updated -= Collection_Updated;
                }

                public void DisposeSource()
                {
                    Collection.Dispose();
                }
            }
        }

        private class DynamicCompositionOfEnumerables<TSource, TResult> : IDynamicListSource<TResult>, IReadOnlyList<TResult>
        {
            private IDynamicListSource<TSource> src;
            private List<List<TResult>> cache = new List<List<TResult>>();
            private Func<TSource, IEnumerable<TResult>> selector;
            private bool propogateDispose;

            public DynamicCompositionOfEnumerables(
                IDynamicListSource<TSource> src,
                Func<TSource, IEnumerable<TResult>> selector,
                bool propogateDispose)
            {
                this.src = src;
                this.selector = selector;
                this.propogateDispose = propogateDispose;

                foreach (var srcElement in src.Snapshot)
                    InsertCollection(cache.Count - 1, srcElement);

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
                    var collection = selector(src.Snapshot[collectionIndex]);
                    return cache[collectionIndex][localIndex];
                }
            }

            public IReadOnlyList<TSource> SourceCollections { get { return src.Snapshot; } }

            public event ListUpdateHandler<TResult> Updated;

            public IEnumerator<TResult> GetEnumerator()
            {
                foreach (var collection in cache)
                {
                    foreach (var element in collection)
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
                OnUpdated(new ListUpdateArgs<TResult>(this, DLinqAction.Dispose));
                if (propogateDispose)
                    src.Dispose();
            }

            private void InsertCollection(int index, TSource srcItem)
            {
                var collection = selector(srcItem);
                var position = GetCollectionStartPosition(index);
                int size = 0;

                foreach (TResult item in collection)
                {
                    OnUpdated(new ListUpdateArgs<TResult>(this, DLinqAction.Insert, position + size, item));
                    size++;
                }

                cache.Insert(index, collection.ToList());
                Count += size;
            }

            private void RemoveCollectionAt(int index)
            {
                var removedCollection = cache[index];
                var position = GetCollectionStartPosition(index);
                var size = removedCollection.Count;
                
                cache.RemoveAt(index);

                foreach (var item in removedCollection)
                    OnUpdated(new ListUpdateArgs<TResult>(this, DLinqAction.Remove, position, default(TResult), item));

                Count -= size;
            }

            private void OnUpdated(ListUpdateArgs<TResult> args)
            {
                if (Updated != null)
                    Updated(args);
            }

            private void CollectionList_Updated(ListUpdateArgs<TSource> args)
            {
                if (args.Action == DLinqAction.Dispose)
                    Dispose();
                else if (args.Action == DLinqAction.Insert)
                    InsertCollection(args.Index, args.NewItem);
                else if (args.Action == DLinqAction.Remove)
                    RemoveCollectionAt(args.Index);
                else if (args.Action == DLinqAction.Replace)
                {
                    RemoveCollectionAt(args.Index);
                    InsertCollection(args.Index, args.NewItem);
                }
            }

            private int ToLocalIndex(int globalIndex, out int collectionIndex)
            {
                int start = 0;

                for (int i = 0; i < src.Snapshot.Count; i++)
                {
                    var end = start + cache[i].Count;

                    if (globalIndex >= start && globalIndex < end)
                    {
                        collectionIndex = i;
                        return globalIndex - start;
                    }
                }

                throw new Exception("Global index out of boundaries! Possible concurrency problems!");
            }

            private int GetCollectionStartPosition(int index)
            {
                int start = 0;

                for (int i = 0; i < index; i++)
                    start += cache[i].Count;

                return start;
            }
        }

        private class CopyProxy<T> : ChainDisposeBase
        {
            private IDynamicListSource<T> src;
            private IList target;

            public CopyProxy(IDynamicListSource<T> src, IList target)
            {
                this.src = src;
                this.target = target;

                foreach (T item in src.Snapshot)
                    target.Add(item);

                src.Updated += Src_Updated;
            }

            private void Src_Updated(ListUpdateArgs<T> args)
            {
                if (args.Action == DLinqAction.Dispose)
                    Dispose();
                else if (args.Action == DLinqAction.Insert)
                    target.Insert(args.Index, args.NewItem);
                else if (args.Action == DLinqAction.Remove)
                    target.RemoveAt(args.Index);
                else if (args.Action == DLinqAction.Replace)
                    target[args.Index] = args.NewItem;
            }

            protected override void DoDispose()
            {
                this.src.Updated -= Src_Updated;
            }

            protected override void SendDisposeToConsumers()
            {
            }

            protected override void SendDisposeToSources()
            {
                src.Dispose();
            }
        }

        private class StaticProxy<T> : ChainDisposeBase, IDynamicListSource<T>
        {
            private IReadOnlyList<T> list;

            public StaticProxy(IReadOnlyList<T> list)
            {
                this.list = list;
            }

            public IReadOnlyList<T> Snapshot { get { return list; } }

            public event ListUpdateHandler<T> Updated;

            protected override void DoDispose()
            {
            }

            protected override void SendDisposeToConsumers()
            {
                if(Updated != null)
                    Updated(new ListUpdateArgs<T>(this, DLinqAction.Dispose));
            }

            protected override void SendDisposeToSources()
            {
            }
        }
    }
}
