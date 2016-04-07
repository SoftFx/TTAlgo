using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class EmptyBuffer<T> : IPluginDataBuffer<T>
    {
        public int Count { get { return 0; } }
        public int VirtualPos { get { return 0; } }
        public T this[int index] { get { return default(T); } set { } }
    }
}
