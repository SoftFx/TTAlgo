using System.Threading.Channels;

namespace TickTrader.Algo.Async
{
    public static class DefaultChannelFactory
    {
        public static Channel<T> CreateUnbounded<T>(bool singleReader = false, bool singleWriter = false, bool allowSynchronousContinuations = false)
        {
            return Channel.CreateUnbounded<T>(new UnboundedChannelOptions { SingleReader = singleReader, SingleWriter = singleWriter, AllowSynchronousContinuations = allowSynchronousContinuations });
        }

        public static Channel<T> CreateForSingleConsumer<T>()
        {
            return CreateUnbounded<T>(true);
        }

        public static Channel<T> CreateForEvent<T>()
        {
            return CreateUnbounded<T>(true, true);
        }
    }
}
