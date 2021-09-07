using System.Collections.Generic;

namespace TickTrader.Algo.Core
{
    public class BuffersCoordinator
    {
        private readonly List<IBuffer> _buffers = new List<IBuffer>();

        public int VirtualPos { get; private set; }
        public int MaxBufferSize { get; set; }

        public void RegisterBuffer(IBuffer buffer)
        {
            _buffers.Add(buffer);
        }

        public void Extend()
        {
            VirtualPos++;
            foreach (var buffer in _buffers)
                buffer.Extend();
        }

        public void BeginBatch()
        {
            foreach (var buffer in _buffers)
                buffer.BeginBatch();
        }

        public void EndBatch()
        {
            foreach (var buffer in _buffers)
                buffer.EndBatch();
        }

        public void Truncate(int bySize)
        {
            foreach (var buffer in _buffers)
                buffer.Truncate(bySize);

            VirtualPos -= bySize;
        }

        public void Clear()
        {
            //BuffersCleared();

            foreach (var buffer in _buffers)
                buffer.Clear();

            _buffers.Clear();

            VirtualPos = 0;
        }
    }
}
