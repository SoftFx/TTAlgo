using System;

namespace TickTrader.Algo.Core
{
    public class ProxyBuffer2<TSrc, TDst> : IPluginDataBuffer<TDst>
    {
        private Func<TSrc, TSrc, TDst> _readTransform;

        public ProxyBuffer2(Func<TSrc, TSrc, TDst> readTransform)
            : this(null, null, readTransform)
        {
        }

        public ProxyBuffer2(IPluginDataBuffer<TSrc> srcBuffer1, IPluginDataBuffer<TSrc> srcBuffer2, Func<TSrc, TSrc, TDst> readTransform)
        {
            SrcBuffer1 = srcBuffer1;
            SrcBuffer2 = srcBuffer2;
            _readTransform = readTransform;
        }

        public int Count { get { return Math.Max(SrcBuffer1.Count, SrcBuffer2.Count); } }
        public IPluginDataBuffer<TSrc> SrcBuffer1 { get; set; }
        public IPluginDataBuffer<TSrc> SrcBuffer2 { get; set; }

        int IPluginDataBuffer<TDst>.VirtualPos { get { return SrcBuffer1.VirtualPos; } }

        public TDst this[int index]
        {
            get { return _readTransform(SrcBuffer1[index], SrcBuffer2[index]); }
            set { }
        }
    }
}
