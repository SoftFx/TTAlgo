using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    partial class VarDictionary<TKey, TValue>
    {
        private class KeyCollection : IVarSet<TKey>
        {
            public KeyCollection(IEnumerable<TKey> snapshot)
            {
                Snapshot = snapshot;
            }

            public IEnumerable<TKey> Snapshot { get; }
            public event SetUpdateHandler<TKey> Updated;

            internal void FireKeyAdded(TKey key)
            {
                Updated?.Invoke(new SetUpdateArgs<TKey>(this, DLinqAction.Insert, key));
            }

            internal void FireKeyRemoved(TKey key)
            {
                Updated?.Invoke(new SetUpdateArgs<TKey>(this, DLinqAction.Remove, default(TKey), key));
            }

            public void Dispose()
            {
            }
        }     
    }
}
