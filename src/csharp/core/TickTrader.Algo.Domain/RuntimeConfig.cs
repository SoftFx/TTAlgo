using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;

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

        public void InitTimeSpanBuffering(DateTime from, DateTime to)
        {
            BufferStrategyConfig = Any.Pack(new TimeSpanStrategyConfig { From = from.ToUniversalTime().ToTimestamp(), To = to.ToUniversalTime().ToTimestamp() });
        }

        public void InitPriorityInvokeStrategy()
        {
            InvokeStrategyConfig = Any.Pack(new PriorityInvokeStrategyConfig());
        }

        public void SetMainSeries(IEnumerable<BarData> bars)
        {
            var barChunk = new BarChunk();
            barChunk.Bars.AddRange(bars);
            MainSeries = Any.Pack(barChunk);
        }
    }
}
