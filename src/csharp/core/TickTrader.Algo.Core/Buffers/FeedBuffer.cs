using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface IFeedBuffer
    {
        UtcTicks this[int index] { get; }

        int Count { get; }

        bool IsMain { get; set; }
    }

    public interface IFeedBuffer<T>
    {
        T this[int index] { get; }

        int Count { get; }
    }

    internal interface ILoadableFeedBuffer : IFeedBuffer
    {
        bool IsLoaded { get; }


        Task LoadFeed(UtcTicks from, int count);

        Task LoadFeed(UtcTicks from, UtcTicks to);
    }

    internal interface IWritableFeedBuffer<T> : ILoadableFeedBuffer
    {
        void ApplyUpdate(T update);
    }


    internal class FeedBufferBase<T>
    {
        protected readonly CircularList<T> _data = new CircularList<T>();
        protected readonly IFeedControllerContext _context;


        public T this[int index] => _data[index];

        public int Count => _data.Count;

        public bool IsLoaded { get; protected set; }

        public bool IsMain { get; set; }


        public FeedBufferBase(IFeedControllerContext context)
        {
            _context = context;
        }


        public void ApplySnapshot(IEnumerable<T> data)
        {
            _data.Clear();
            _data.AddRange(data);
        }
    }
}
