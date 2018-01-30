using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.Algo.Common.Lib
{
    public class DictionarySyncrhonizer<TKey, TValue> : IVarSet<TKey, TValue>
    {
        private ISyncContext _sync;
        private IVarSet<TKey, TValue> _srcCollection;
        private Dictionary<TKey, TValue> _collectionCopy;
        private bool _isDiposed;

        public DictionarySyncrhonizer(IVarSet<TKey, TValue> srcCollection, ISyncContext sync)
        {
            _srcCollection = srcCollection;
            _collectionCopy = srcCollection.Snapshot.ToDictionary(e => e.Key, e => e.Value);
            _sync = sync;
            _srcCollection.Updated += _srcCollection_Updated;
        }

        public IReadOnlyDictionary<TKey, TValue> Snapshot => _collectionCopy;
        public event DictionaryUpdateHandler<TKey, TValue> Updated;

        private void _srcCollection_Updated(DictionaryUpdateArgs<TKey, TValue> args)
        {
            _sync.Invoke(() =>
            {
                if (!_isDiposed)
                {
                    if (args.Action == DLinqAction.Insert)
                        _collectionCopy.Add(args.Key, args.NewItem);
                    else if (args.Action == DLinqAction.Remove)
                        _collectionCopy.Remove(args.Key);
                    else if (args.Action == DLinqAction.Replace)
                        _collectionCopy[args.Key] = args.NewItem;
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
