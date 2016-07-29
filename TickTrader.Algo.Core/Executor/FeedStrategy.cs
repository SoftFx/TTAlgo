using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public abstract class FeedStrategy
    {
        internal FeedStrategy()
        {
        }

        public abstract int BufferSize { get; }
        public abstract ITimeRef TimeRef { get; }
        internal abstract void Start(PluginExecutor executor);
        public abstract BufferUpdateResults UpdateBuffers(FeedUpdate update);
        internal abstract void MapInput<TSrc, TVal>(string inputName, string symbolCode, Func<TSrc, TVal> selector);
        internal abstract void Stop();
    }
}
