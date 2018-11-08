using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    public class VarSet<T> : IVarSet<T>
    {
        private HashSet<T> _innerSet;

        public IEnumerable<T> Snapshot => _innerSet;

        public VarSet()
        {
            _innerSet = new HashSet<T>();
        }

        public VarSet(IEqualityComparer<T> comparer)
        {
            _innerSet = new HashSet<T>(comparer);
        }

        public void Add(T item)
        {
            _innerSet.Add(item);
            Updated?.Invoke(new SetUpdateArgs<T>(this, DLinqAction.Insert, item));
        }

        public bool Remove(T item)
        {
            if (_innerSet.Remove(item))
            {
                Updated?.Invoke(new SetUpdateArgs<T>(this, DLinqAction.Remove, default(T), item));
                return true;
            }

            return false;
        }

        public event SetUpdateHandler<T> Updated;

        public void Dispose()
        {
        }
    }
}
