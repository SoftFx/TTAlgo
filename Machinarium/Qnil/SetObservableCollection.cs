using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class SetObservableCollection<T> : ObservableCollection<T>, IObservableListSource<T>
    {
        private IDynamicSetSource<T> _set;
        private bool propogateDispose;
        private bool isDisposed;

        public SetObservableCollection(IDynamicSetSource<T> set, bool propogateDispose)
            : base(set.Snapshot)
        {
            _set = set;
            this.propogateDispose = propogateDispose;
            _set.Updated += _set_Updated;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                _set.Updated -= _set_Updated;

                isDisposed = true;

                if (propogateDispose)
                    _set.Dispose();
            }
        }

        private void _set_Updated(SetUpdateArgs<T> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else if (args.Action == DLinqAction.Insert)
                Add(args.NewItem);
            else if (args.Action == DLinqAction.Remove)
                Remove(args.OldItem);
            else if (args.Action == DLinqAction.Replace)
            {
                // ??
            }
        }
    }
}
