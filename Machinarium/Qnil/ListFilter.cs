using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class ListFilter<T> : IVarList<T>, IReadOnlyList<T>
    {
        private IVarList<T> src;
        private List<int> indexMap = new List<int>();
        private Predicate<T> filterPredicate;
        private bool propogateDispose;

        public ListFilter(IVarList<T> src, Predicate<T> filterPredicate, bool propogateDispose)
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
}
