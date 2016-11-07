using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class ObservableWrapper2<T> : ObservableCollection<T>, IObservableListSource<T>
    {
        private IDynamicListSource<T> src;
        private bool propogateDispose;
        private bool isDisposed;

        public ObservableWrapper2(IDynamicListSource<T> src, bool propogateDispose)
            : base(src.Snapshot)
        {
            this.src = src;
            this.propogateDispose = propogateDispose;

            src.Updated += Src_Updated;
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

        private void Src_Updated(ListUpdateArgs<T> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
                Insert(args.Index, args.NewItem);
            else if (args.Action == DLinqAction.Remove)
                RemoveAt(args.Index);
            else if (args.Action == DLinqAction.Replace)
                this[args.Index] = args.NewItem;
        }
    }
}
