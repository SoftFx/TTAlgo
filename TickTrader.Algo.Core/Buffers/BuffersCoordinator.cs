using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class BuffersCoordinator
    {
        private List<IBuffer> buffers = new List<IBuffer>();

        public int VirtualPos { get; private set; }
        public int MaxBufferSize { get; set; }

        internal void RegisterBuffer(IBuffer buffer)
        {
            buffers.Add(buffer);
        }

        public void Extend()
        {
            VirtualPos++;
            foreach (var buffer in buffers)
                buffer.Extend();
        }

        public void BeginBatch()
        {
            foreach (var buffer in buffers)
                buffer.BeginBatch();
        }

        public void EndBatch()
        {
            foreach (var buffer in buffers)
                buffer.EndBatch();
        }

        public void Truncate(int bySize)
        {
            foreach (var buffer in buffers)
                buffer.Truncate(bySize);
        }

        //public void Reset()
        //{
        //    BuffersCleared();
        //    VirtualPos = 0;
        //}
    }
}
