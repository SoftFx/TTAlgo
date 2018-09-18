using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public abstract class FeedBufferStrategy
    {
        protected IFeedBuffer MainBuffer { get; private set; }
        private IFeedBuferStrategyContext Context { get; set; }

        internal void Init(IFeedBuferStrategyContext context)
        {
            Context = context;
            MainBuffer = context.MainBuffer;
        }

        public abstract void OnBufferExtended();
        public abstract void Start();
        public abstract void InitBuffer(IFeedLoader buffer);
        public abstract bool InBoundaries(DateTime timePoint);

        protected void TruncateBuffers(int bySize)
        {
            Context.TruncateBuffers(bySize);
        }
    }

    public interface IFeedLoader
    {
        bool IsLoaded { get; }

        void LoadFeed(DateTime from, DateTime to);
        void LoadFeed(int size);
        void LoadFeed(DateTime from, int size);
    }

    public interface IFeedBuffer : IFeedLoader, ITimeRef
    {
    }

    internal interface IFeedBuferStrategyContext
    {
        IFeedBuffer MainBuffer { get; }
        void TruncateBuffers(int bySize);
    }

    public class SlidingBufferStrategy : FeedBufferStrategy
    {
        private int _size;

        public SlidingBufferStrategy(int size)
        {
            _size = size;
        }

        public override void Start()
        {
            if (!MainBuffer.IsLoaded)
                MainBuffer.LoadFeed(_size);
        }

        public override void OnBufferExtended()
        {
            var bufferSize = MainBuffer.LastIndex + 1;

            if (bufferSize > _size)
            {
                var trucateBy = bufferSize - _size;
                TruncateBuffers(trucateBy);
            }
        }

        public override void InitBuffer(IFeedLoader buffer)
        {
            if (!buffer.IsLoaded)
                buffer.LoadFeed(_size);
        }

        public override bool InBoundaries(DateTime timePoint)
        {
            return true; // do not need to check boundaries in this strategy
        }
    }

    public class TimeSpanStrategy : FeedBufferStrategy
    {
        private DateTime _from;
        private DateTime _to;

        public TimeSpanStrategy(DateTime from, DateTime to)
        {
            _from = from;
            _to = to;
        }

        public override void Start()
        {
            if (!MainBuffer.IsLoaded)
                MainBuffer.LoadFeed(_from, _to);
        }

        public override void OnBufferExtended()
        {
        }

        public override void InitBuffer(IFeedLoader buffer)
        {
            buffer.LoadFeed(_from, _to);
        }

        public override bool InBoundaries(DateTime timePoint)
        {
            return timePoint >= _from || timePoint <= _to;
        }
    }
}
