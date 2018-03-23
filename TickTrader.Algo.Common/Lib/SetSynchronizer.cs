using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.Algo.Common.Lib
{
    public class SetSynchronizer<T> : IVarSet<T>
    {
        private ISyncContext _sync;
        private IVarSet<T> _srcCollection;
        private HashSet<T> _collectionCopy;
        private bool _isDiposed;

        public SetSynchronizer(IVarSet<T> srcCollection, ISyncContext sync)
        {
            _srcCollection = srcCollection;
            _collectionCopy = new HashSet<T>(srcCollection.Snapshot);
            _sync = sync;
            _srcCollection.Updated += _srcCollection_Updated;
        }

        public IEnumerable<T> Snapshot => _collectionCopy;
        public event SetUpdateHandler<T> Updated;

        private void _srcCollection_Updated(SetUpdateArgs<T> args)
        {
            _sync.Invoke(() =>
            {
                if (!_isDiposed)
                {
                    if (args.Action == DLinqAction.Insert)
                        _collectionCopy.Add(args.NewItem);
                    else if (args.Action == DLinqAction.Remove)
                        _collectionCopy.Remove(args.OldItem);
                    Updated?.Invoke(args);
                }
            });
        }

        public void Dispose()
        {
            _sync.Invoke(() =>
            {
                _isDiposed = true;
                _srcCollection.Updated -= _srcCollection_Updated;
            });
        }
    }
}
