using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Lib
{
    public class ObservableComposer<T> : ObservableCollection<T>
    {
        public ObservableComposer()
        {
            SourceCollections = new CollectionList(this);
        }

        public CollectionList SourceCollections { get; private set; }

        private void CollectionsCleared(IEnumerable<ObservableCollection<T>> clearedList)
        {
            foreach(var collection in clearedList)
                collection.CollectionChanged -= Collection_CollectionChanged;

            this.Clear();
        }

        private void CollectionRemoved(ObservableCollection<T> collection, int index)
        {
            collection.CollectionChanged -= Collection_CollectionChanged;
            int startIndex = GetCollectionStartPosition(index);
            RemoveRange(startIndex, collection.Count);
        }

        private void CollectionInserted(ObservableCollection<T> collection, int index)
        {
            collection.CollectionChanged += Collection_CollectionChanged;
            int startIndex =  GetCollectionStartPosition(index);
            AddRange(startIndex, collection);
        }

        private void CollectionReplaced(ObservableCollection<T> oldCollection, ObservableCollection<T> newCollection)
        {
            int startIndex = GetCollectionStartPosition(newCollection);
            int toRemove = oldCollection.Count;
            RemoveRange(startIndex, toRemove);
            AddRange(startIndex, newCollection);
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int insertIndex = GetCollectionStartPosition((ObservableCollection<T>)sender) + e.NewStartingIndex;
                    this.Insert(insertIndex, (T)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    int removeIndex = GetCollectionStartPosition((ObservableCollection<T>)sender) + e.OldStartingIndex;
                    this.RemoveAt(removeIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    int replaceIndex = GetCollectionStartPosition((ObservableCollection<T>)sender) + e.NewStartingIndex;
                    this[replaceIndex] = (T)e.NewItems[0];
                    break;
                default: throw new NotImplementedException("Action '" + e.Action + "' is not supported by ObservableComposer");
            }
        }

        private void RemoveRange(int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
                this.RemoveAt(startIndex);
        }

        private void AddRange(int startIndex, IEnumerable<T> range)
        {
            foreach (T item in range)
                this.Insert(startIndex++, item);
        }

        private int GetCollectionStartPosition(int collectionIndex)
        {
            int start = 0;
            for (int i = 0; i < collectionIndex; i++)
                start += SourceCollections[i].Count;
            return start;
        }

        private int GetCollectionStartPosition(ObservableCollection<T> collection)
        {
            int start = 0;

            for (int i = 0; i < SourceCollections.Count; i++)
            {
                if (SourceCollections[i] == collection)
                    return start;

                start += SourceCollections[i].Count;
            }

            throw new Exception("Collection has been already removed! Possible conccurency problem.");
        }

        public class CollectionList : IList<ObservableCollection<T>>
        {
            private ObservableComposer<T> parent;
            private List<ObservableCollection<T>> innerList = new List<ObservableCollection<T>>();

            public CollectionList(ObservableComposer<T> parent)
            {
                this.parent = parent;
            }

            public ObservableCollection<T> this[int index]
            {
                get { return innerList[index]; }
                set
                {
                    var oldCollection = innerList[index];
                    innerList[index] = value;
                    parent.CollectionReplaced(oldCollection, value);
                }
            }

            public int Count { get { return innerList.Count; } }

            public bool IsReadOnly { get { return false; } }

            public void Add(ObservableCollection<T> item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                innerList.Add(item);
                parent.CollectionInserted(item, innerList.Count - 1);
            }

            public void Clear()
            {
                parent.CollectionsCleared(this.innerList);
                innerList.Clear();
            }

            public bool Contains(ObservableCollection<T> item)
            {
                return innerList.Contains(item);
            }

            public void CopyTo(ObservableCollection<T>[] array, int arrayIndex)
            {
                innerList.CopyTo(array, arrayIndex);
            }

            public IEnumerator<ObservableCollection<T>> GetEnumerator()
            {
                return innerList.GetEnumerator();
            }

            public int IndexOf(ObservableCollection<T> item)
            {
                return innerList.IndexOf(item);
            }

            public void Insert(int index, ObservableCollection<T> item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                innerList.Insert(index, item);
                parent.CollectionInserted(item, index);
            }

            public bool Remove(ObservableCollection<T> item)
            {
                int index =  innerList.IndexOf(item);

                if(index > 0)
                {
                    parent.CollectionRemoved(item, index);
                    return true;
                }
                return false;
            }

            public void RemoveAt(int index)
            {
                var collection = innerList[index];
                innerList.RemoveAt(index);
                parent.CollectionRemoved(collection, index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return innerList.GetEnumerator();
            }
        }
    }
}
