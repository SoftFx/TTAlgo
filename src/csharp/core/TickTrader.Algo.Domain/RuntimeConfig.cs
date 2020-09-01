using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain
{
    public partial class RuntimeConfig
    {
        public void InitBarStrategy(Feed.Types.MarketSide marketSide)
        {
            FeedStrategyConfig = Any.Pack(new BarStratefyConfig { MarketSide = marketSide });
        }

        public void InitQuoteStrategy()
        {
            FeedStrategyConfig = Any.Pack(new QuoteStrategyConfig());
        }

        public void InitSlidingBuffering(int size)
        {
            BufferStrategyConfig = Any.Pack(new SlidingBufferStrategyConfig { Size = size });
        }

        public void InitTimeSpanBuffering(Timestamp from, Timestamp to)
        {
            BufferStrategyConfig = Any.Pack(new TimeSpanStrategyConfig { From = from, To = to });
        }
    }
}
