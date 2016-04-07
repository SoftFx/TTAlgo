using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class ProxyBuffer<TSrc, TDst> : IPluginDataBuffer<TDst>
    {
        private IPluginDataBuffer<TSrc> srcBuffer;
        private Func<TSrc, TDst> readTransform;
        private Func<TSrc, TDst, TSrc> writeTransform = null;

        public ProxyBuffer(Func<TSrc, TDst> readTransform, Func<TSrc, TDst, TSrc> writeTransform = null)
            : this(null, readTransform, writeTransform)
        {
        }

        public ProxyBuffer(IPluginDataBuffer<TSrc> srcBuffer, Func<TSrc, TDst> readTransform, Func<TSrc, TDst, TSrc> writeTransform = null)
        {
            this.srcBuffer = srcBuffer;
            this.readTransform = readTransform;
            this.writeTransform = writeTransform;
        }

        public int Count { get { return srcBuffer.Count; } }
        public IPluginDataBuffer<TSrc> SrcBuffer { get { return srcBuffer; } set { srcBuffer = value; } }

        int IPluginDataBuffer<TDst>.VirtualPos { get { return srcBuffer.VirtualPos; } }

        int VirtualPost { get { return srcBuffer.VirtualPos; } }

        public TDst this[int index]
        {
            get { return readTransform(srcBuffer[index]); }
            set
            {
                TSrc srcRecord = srcBuffer[index];
                srcBuffer[index] = writeTransform(srcRecord, value);
            }
        }
    }
}
