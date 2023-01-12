using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class BuffersCoordinator2
    {
        private readonly List<IBuffer> _buffers = new List<IBuffer>();
        private readonly ITimeRef _timeline;

        private int _timelineStart, _timelineEnd;


        public int VirtualPos { get; private set; }

        public int MaxBufferSize { get; set; }


        public BuffersCoordinator2(ITimeRef timeline)
        {
            _timeline = timeline;
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

        public void Clear()
        {
            foreach (var buffer in _buffers)
                buffer.Clear();

            _buffers.Clear();

            VirtualPos = 0;
        }


        public void Extend(UtcTicks time)
        {
            var bySize = 0;
            var cnt = _timeline.Count;
            var end = _timelineEnd;
            for (; end < cnt && _timeline[end] <= time; end++, bySize++) ;
            _timelineEnd = end;

            if (bySize == 0)
                return;

            for (var i = 0; i < bySize; i++)
            {
                VirtualPos++;
                foreach (var buffer in _buffers)
                    buffer.Extend();
            }
        }

        public void Truncate(UtcTicks time)
        {
            var bySize = 0;
            var end = _timelineEnd;
            var start = _timelineStart;
            for (; start < end && _timeline[start] < time; start++, bySize++) ;
            _timelineStart = start;

            if (bySize == 0)
                return;

            foreach (var buffer in _buffers)
                buffer.Truncate(bySize);

            VirtualPos -= bySize;
        }
    }
}
