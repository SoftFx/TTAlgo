using System;

namespace TickTrader.Algo.Core
{
    public class ProxyBuffer<TSrc, TDst> : IPluginDataBuffer<TDst>
    {
        private IPluginDataBuffer<TSrc> _srcBuffer;
        private Func<TSrc, TDst> _readTransform;
        private Func<TSrc, TDst, TSrc> _writeTransform = null;

        public ProxyBuffer(Func<TSrc, TDst> readTransform, Func<TSrc, TDst, TSrc> writeTransform = null)
            : this(null, readTransform, writeTransform)
        {
        }

        public ProxyBuffer(IPluginDataBuffer<TSrc> srcBuffer, Func<TSrc, TDst> readTransform, Func<TSrc, TDst, TSrc> writeTransform = null)
        {
            _srcBuffer = srcBuffer;
            _readTransform = readTransform;
            _writeTransform = writeTransform;
        }

        public int Count { get { return _srcBuffer.Count; } }
        public IPluginDataBuffer<TSrc> SrcBuffer { get { return _srcBuffer; } set { _srcBuffer = value; } }

        int IPluginDataBuffer<TDst>.VirtualPos { get { return _srcBuffer.VirtualPos; } }

        public TDst this[int index]
        {
            get { return _readTransform(_srcBuffer[index]); }
            set
            {
                TSrc srcRecord = _srcBuffer[index];
                _srcBuffer[index] = _writeTransform(srcRecord, value);
            }
        }
    }
}
