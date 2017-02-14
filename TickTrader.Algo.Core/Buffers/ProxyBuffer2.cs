using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class ProxyBuffer2<TSrc, TDst> : IPluginDataBuffer<TDst>
    {
        private Func<TSrc, TSrc, TDst> readTransform;

        public ProxyBuffer2(Func<TSrc, TSrc, TDst> readTransform)
            : this(null, null, readTransform)
        {
        }

        public ProxyBuffer2(IPluginDataBuffer<TSrc> srcBuffer1, IPluginDataBuffer<TSrc> srcBuffer2, Func<TSrc, TSrc, TDst> readTransform)
        {
            this.SrcBuffer1 = srcBuffer1;
            this.SrcBuffer2 = srcBuffer2;
            this.readTransform = readTransform;
        }

        public int Count { get { return System.Math.Max(SrcBuffer1.Count, SrcBuffer2.Count); } }
        public IPluginDataBuffer<TSrc> SrcBuffer1 { get; set; }
        public IPluginDataBuffer<TSrc> SrcBuffer2 { get; set; }

        int IPluginDataBuffer<TDst>.VirtualPos { get { return SrcBuffer1.VirtualPos; } }

        public TDst this[int index]
        {
            get { return readTransform(SrcBuffer1[index], SrcBuffer2[index]); }
            set { }
        }
    }
}
