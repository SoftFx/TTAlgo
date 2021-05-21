using System;

namespace TickTrader.Algo.Core
{
    public abstract class BufferProxy<TSrc, TDst> : IPluginDataBuffer<TDst>
    {
        private BuffersCoordinator _coordinator;
        private InputBuffer<TSrc> _srcSeries;

        protected BufferProxy(InputBuffer<TSrc> srcSeries)
        {
            _coordinator = srcSeries.Coordinator;
            _srcSeries = srcSeries;
        }

        protected abstract TDst Convert(TSrc srcItem);

        public TDst this[int index]
        {
            get { return Convert(_srcSeries[index]); }
            set { throw new NotSupportedException("Proxy buffers cannot be modified directly!"); }
        }

        public int Count { get { return _srcSeries.Count; } }
        public int VirtualPos { get { return _coordinator.VirtualPos; } }
    }
}
