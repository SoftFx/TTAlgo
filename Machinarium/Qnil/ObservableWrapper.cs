using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Machinarium.Qnil
{
    internal class ObservableWrapper<T> : IObservableListSource<T>
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

        public T this[int index] { get { return src.Snapshot[index]; } set { } }

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
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, args.NewItem, args.Index));
            }
            else if (args.Action == DLinqAction.Remove)
            {
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldItem, args.Index));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace, args.NewItem, args.OldItem, args.Index));
            }
        }

        #region Implementing IList just to be compatible with ObservableCollection

        public bool IsReadOnly { get { return true; } }
        public bool IsFixedSize { get { return false; } }
        public object SyncRoot { get { throw new NotImplementedException(); } }
        public bool IsSynchronized { get { return false; } }

        object IList.this[int index] { get { return src.Snapshot[index]; } set { throw new NotImplementedException(); } }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            return src.Snapshot.Any(v => Compare(v, value));
        }

        private bool Compare(object a, object b)
        {
            if (a == null)
                return b == null;
            if (b == null)
                return false;
            if (ReferenceEquals(a, b))
                return true;
            return a.Equals(b);
        }

        public int IndexOf(object value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Compare(this[i], value))
                    return i;
            }
            return -1;
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
