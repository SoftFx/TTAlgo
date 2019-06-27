using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common
{
    public struct EntityUpdateArgs<T>
    {
        public EntityUpdateArgs(T val, EntityUpdateActions action)
        {
            Action = action;
            Entity = val;
        }

        public T Entity { get; }
        public EntityUpdateActions Action { get; }
    }

    public enum EntityUpdateActions { Add, Remove, Replace }
}
