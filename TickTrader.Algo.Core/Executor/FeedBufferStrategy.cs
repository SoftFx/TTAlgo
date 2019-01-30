using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public abstract class FeedBufferStrategy
    {
        private List<ILoadableFeedBuffer> _toLoad = new List<ILoadableFeedBuffer>();

        protected IFeedBuffer MainBuffer { get; private set; }
        protected bool IsStarted { get; private set; }
        private IFeedBuferStrategyContext Context { get; set; }

        internal void Init(IFeedBuferStrategyContext context)
        {
            Context = context;
            MainBuffer = context.MainBuffer;

            _toLoad.Add(MainBuffer);
        }

        public abstract void OnBufferExtended();
        public abstract bool InBoundaries(DateTime timePoint);
        public abstract void OnUserSetBufferSize(int newSize, out string error);

        protected abstract void LoadData(ILoadableFeedBuffer buffer);

        public void Start()
        {
            foreach (var buff in _toLoad)
            {
                if (!buff.IsLoaded) // buffers can have pre-loaded data
                    LoadData(buff);
            }

            _toLoad.Clear();

            IsStarted = true;
        }

        public void InitBuffer(ILoadableFeedBuffer buffer)
        {
            if (IsStarted)
                LoadData(buffer);
            else if(!_toLoad.Contains(buffer))
                _toLoad.Add(buffer);
        }

        protected void TruncateBuffers(int bySize)
        {
            Context.TruncateBuffers(bySize);
        }
    }

    public interface ILoadableFeedBuffer
    {
        bool IsLoaded { get; }

        void LoadFeed(DateTime from, DateTime to);
        void LoadFeed(int size);
        void LoadFeed(DateTime from, int size);
    }

    public interface IFeedBuffer : ILoadableFeedBuffer, ITimeRef
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

        protected override void LoadData(ILoadableFeedBuffer buffer)
        {
            buffer.LoadFeed(_size);
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

        public override bool InBoundaries(DateTime timePoint)
        {
            return true; // do not need to check boundaries in this strategy
        }

        public override void OnUserSetBufferSize(int newSize, out string error)
        {
            if (IsStarted)
                error = "SetInputSize() can be called only during initialization! Please call it from Init() override.";
            else if (newSize <= 1)
                error = "SetInputSize() : New size must be greater than 1!";
            else
            {
                error = null;
                _size = newSize;
            }
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

        protected override void LoadData(ILoadableFeedBuffer buffer)
        {
            buffer.LoadFeed(_from, _to);
        }

        public override void OnBufferExtended()
        {
        }

        public override bool InBoundaries(DateTime timePoint)
        {
            return timePoint >= _from || timePoint <= _to;
        }

        public override void OnUserSetBufferSize(int newSize, out string error)
        {
            error = "SetInputSize() is not supported!";
        }
    }
}
