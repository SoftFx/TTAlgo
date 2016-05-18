using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public static class DLinq
    {
        public static IDynamicList<TOut> DynamicSelect<TIn, TOut>(this IDynamicList<TIn> src, Func<TIn, TOut> selector)
        {
            return new ListSelector<TIn, TOut>(src, selector, false);
        }

        public static IDynamicList<TOut> DynamicSelectChain<TIn, TOut>(this IDynamicList<TIn> src, Func<TIn, TOut> selector)
        {
            return new ListSelector<TIn, TOut>(src, selector, true);
        }

        public static IDynamicList<T> DynamicWhere<T>(this IDynamicList<T> src, Predicate<T> condition)
        {
            return new ListFilter<T>(src, condition, false);
        }

        public static IDynamicList<T> DynamicWhereChain<T>(this IDynamicList<T> src, Predicate<T> condition)
        {
            return new ListFilter<T>(src, condition, true);
        }

        public static IDynamicList<T> ToDynamicList<T>(this IDynamicList<T> src)
        {
            return new ProjectionList<T>(src, false);
        }

        public static IDynamicList<T> ToDynamicListChain<T>(this IDynamicList<T> src)
        {
            return new ProjectionList<T>(src, true);
        }

        public static IObservableList<T> AsObservable<T>(this IDynamicList<T> src)
        {
            return new ObservableWrapper<T>(src, false);
        }

        public static IObservableList<T> AsObservableChain<T>(this IDynamicList<T> src)
        {
            return new ObservableWrapper<T>(src, false);
        }

        public static IObservableList<T> ToObservableList<T>(this IDynamicList<T> src)
        {
            return src.ToDynamicList().AsObservableChain();
        }

        public static IObservableList<T> ToObservableListChain<T>(this IDynamicList<T> src)
        {
            return src.ToDynamicListChain().AsObservableChain();
        }

        private class ListSelector<TIn, TOut> : IDynamicList<TOut>
        {
            private IDynamicList<TIn> src;
            private Func<TIn, TOut> selectFunc;
            private bool propogateDispose;

            public ListSelector(IDynamicList<TIn> src, Func<TIn, TOut> selectFunc, bool propogateDispose)
            {
                this.src = src;
                this.selectFunc = selectFunc;
                this.propogateDispose = propogateDispose;

                src.Updated += Src_Updated;
            }

            public int Count { get { return src.Count; } }
            public TOut this[int index] { get { return selectFunc(src[index]); } }

            public event ListUpdateHandler<TOut> Updated;

            public IEnumerator<TOut> GetEnumerator()
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
                OnUpdated(new ListUpdateArgs<TOut>(this, DLinqUpdateType.Dispose));
                if (propogateDispose)
                    src.Dispose();
            }

            private void OnUpdated(ListUpdateArgs<TOut> args)
            {
                if (Updated != null)
                    Updated(args);
            }

            private void Src_Updated(ListUpdateArgs<TIn> args)
            {
                if (args.Action == DLinqUpdateType.Dispose)
                    Dispose();
                else if (args.Action == DLinqUpdateType.RemoveAll)
                    OnUpdated(new ListUpdateArgs<TOut>(this, DLinqUpdateType.RemoveAll));
                else
                {
                    TOut newItem = default(TOut);
                    TOut oldItem = default(TOut);

                    if (args.Action == DLinqUpdateType.Insert || args.Action == DLinqUpdateType.Replace)
                        newItem = selectFunc(args.NewItem);

                    if (args.Action == DLinqUpdateType.Remove || args.Action == DLinqUpdateType.Replace)
                        oldItem = selectFunc(args.OldItem);

                    OnUpdated(new ListUpdateArgs<TOut>(this, args.Action, args.Index, newItem, oldItem));
                }
            }
        }

        private class ListFilter<T> : IDynamicList<T>
        {
            private IDynamicList<T> src;
            private List<int> indexMap = new List<int>();
            private Predicate<T> filterPredicate;
            private bool propogateDispose;

            public ListFilter(IDynamicList<T> src, Predicate<T> filterPredicate, bool propogateDispose)
            {
                this.src = src;
                this.filterPredicate = filterPredicate;
                this.propogateDispose = propogateDispose;

                for (int i = 0; i < src.Count; i++)
                {
                    if (filterPredicate(src[i]))
                        indexMap.Add(i);
                }

                src.Updated += Src_Updated;
            }

            public int Count { get { return indexMap.Count; } }
            public T this[int index] { get { return src[indexMap[index]]; } }

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
                OnUpdated(new ListUpdateArgs<T>(this, DLinqUpdateType.Dispose));
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

            private int FindNearestIndexInMap(int srcIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (indexMap[i] >= srcIndex)
                        return i;
                }
                return Count;
            }

            private void Src_Updated(ListUpdateArgs<T> args)
            {
                if (args.Action == DLinqUpdateType.Dispose)
                    Dispose();
                else if (args.Action == DLinqUpdateType.RemoveAll)
                {
                    indexMap.Clear();
                    OnUpdated(new ListUpdateArgs<T>(this, DLinqUpdateType.RemoveAll));
                }
                else if (args.Action == DLinqUpdateType.Insert)
                {
                    if (!filterPredicate(args.NewItem))
                        return;

                    var newIndex = FindNearestIndexInMap(args.Index);
                    indexMap.Insert(newIndex, args.Index);
                    OnUpdated(new ListUpdateArgs<T>(this, DLinqUpdateType.Insert, newIndex, args.NewItem));
                }
                else if (args.Action == DLinqUpdateType.Remove)
                {
                    var mapIndex = FindIndexInMap(args.Index);
                    if (mapIndex >= 0)
                        indexMap.RemoveAt(mapIndex);
                    OnUpdated(new ListUpdateArgs<T>(this, DLinqUpdateType.Remove, mapIndex, default(T), args.OldItem));
                }
                else if (args.Action == DLinqUpdateType.Replace)
                {
                    var mapIndex = FindIndexInMap(args.Index);
                    bool isFiltered = !filterPredicate(args.NewItem);

                    if (mapIndex >= 0)
                    {
                        if (!isFiltered)
                        {
                            // replace
                            indexMap[mapIndex] = args.Index;
                            OnUpdated(new ListUpdateArgs<T>(this, DLinqUpdateType.Replace, mapIndex, args.NewItem, args.OldItem));
                        }
                        else
                        {
                            // remove
                            indexMap.RemoveAt(mapIndex);
                            OnUpdated(new ListUpdateArgs<T>(this, DLinqUpdateType.Remove, mapIndex, default(T), args.OldItem));
                        }
                    }
                    else
                    {
                        if (!isFiltered)
                        {
                            // insert
                            var newIndex = FindNearestIndexInMap(args.Index);
                            indexMap.Insert(newIndex, args.Index);
                            OnUpdated(new ListUpdateArgs<T>(this, DLinqUpdateType.Insert, newIndex, args.NewItem));
                        }
                        else
                        {
                            // do nothing
                        }
                    }
                }
            }
        }

        private class ProjectionList<T> : IDynamicList<T>
        {
            private IDynamicList<T> src;
            private List<T> innerList;
            private bool propogateDispose;

            public ProjectionList(IDynamicList<T> src, bool propogateDispose)
            {
                this.src = src;
                this.innerList = new List<T>(src);
                this.propogateDispose = propogateDispose;

                src.Updated += Src_Updated;
            }

            public int Count { get { return src.Count; } }
            public T this[int index] { get { return innerList[index]; } }

            public event ListUpdateHandler<T> Updated;

            public void Dispose()
            {
                src.Updated -= Src_Updated;
                OnUpdated(new ListUpdateArgs<T>(this, DLinqUpdateType.Dispose));
                if (propogateDispose)
                    src.Dispose();
            }

            private void Src_Updated(ListUpdateArgs<T> args)
            {
                if (args.Action == DLinqUpdateType.Dispose)
                {
                    Dispose();
                    return;
                }
                else if (args.Action == DLinqUpdateType.RemoveAll)
                {
                    innerList.Clear();
                    OnUpdated(new ListUpdateArgs<T>(this, DLinqUpdateType.RemoveAll));
                }
                else if (args.Action == DLinqUpdateType.Insert)
                    innerList.Insert(args.Index, args.NewItem);
                else if (args.Action == DLinqUpdateType.Remove)
                    innerList.RemoveAt(args.Index);
                else if (args.Action == DLinqUpdateType.Replace)
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

        private class ObservableWrapper<T> : IObservableList<T>
        {
            private IDynamicList<T> src;
            private bool propogateDispose;

            public ObservableWrapper(IDynamicList<T> src, bool propogateDispose)
            {
                this.src = src;
                this.propogateDispose = propogateDispose;

                src.Updated += Src_Updated;
            }

            public int Count { get { return src.Count; } }
            public T this[int index] { get { return src[index]; } }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public event PropertyChangedEventHandler PropertyChanged;

            public IEnumerator<T> GetEnumerator()
            {
                return src.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return src.GetEnumerator();
            }

            public void Dispose()
            {
                this.src.Updated -= Src_Updated;

                if (propogateDispose)
                    src.Dispose();
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
                if (args.Action == DLinqUpdateType.Dispose)
                    Dispose();
                else if (args.Action == DLinqUpdateType.RemoveAll)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));
                }
                else if (args.Action == DLinqUpdateType.Insert)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, args.NewItem, args.Index));
                }
                else if (args.Action == DLinqUpdateType.Remove)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, args.OldItem, args.Index));
                }
                else if (args.Action == DLinqUpdateType.Replace)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, args.NewItem, args.OldItem, args.Index));
                }
            }
        }
    }
}
