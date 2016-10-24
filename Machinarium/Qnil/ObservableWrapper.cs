using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

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
                OnPropertyChanged(nameof(Count));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, args.NewItem, args.Index));
            }
            else if (args.Action == DLinqAction.Remove)
            {
                OnPropertyChanged(nameof(Count));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldItem, args.Index));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace, args.NewItem, args.OldItem, args.Index));
            }
        }
    }
}
