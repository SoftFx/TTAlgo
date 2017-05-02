using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.UnitTest
{
    public static class MockHelper
    {
        internal static List<BarEntity> Add(this List<BarEntity> list, DateTime openTime, double open,
            double? close = null, double? high = null, double? low = null)
        {
            var bar = new BarEntity()
            {
                OpenTime = openTime,
                Open = open,
                Close = close ?? open,
                High = high ?? open,
                Low = low ?? open
            };
            list.Add(bar);
            return list;
        }

        internal static List<BarEntity> Add(this List<BarEntity> list, string openTime, double open,
            double? close = null, double? high = null, double? low = null)
        {
            return Add(list, DateTime.Parse(openTime), open, close, high, low);
        }

        internal static Api.Quote CreateQuote(string timestamp, double bid, double? ask)
        {
            return CreateQuote(null, DateTime.Parse(timestamp), bid, ask);
        }

        internal static Api.Quote CreateQuote(string symbol, DateTime timestamp, double bid, double? ask = null)
        {
            return new QuoteEntity()
            {
                Symbol = symbol,
                Bid = bid,
                Ask = ask ?? bid,
                Time = timestamp
            };
        }

        internal static void UpdateRate(this BarSeriesFixture fixture, string timestamp, double bid, double? ask = null)
        {
            fixture.Update(CreateQuote(timestamp, bid, ask));
        }
    }

    internal class MockBot : TradeBot
    {
    }

    internal class MockFixtureContext : IFixtureContext
    {
        public MockFixtureContext()
        {
            Builder = new PluginBuilder(new Metadata.AlgoPluginDescriptor(typeof(MockBot)));
            Logger = new NullLogger();
        }

        public PluginBuilder Builder { get; private set; }
        public IPluginLogger Logger { get; private set; }
        public string MainSymbolCode { get; set; }
        public TimeFrames TimeFrame { get; set; }
        public DateTime TimePeriodEnd { get; set; }
        public DateTime TimePeriodStart { get; set; }

        public void Enqueue(Action<PluginBuilder> action)
        {
        }

        public void Enqueue(QuoteEntity update)
        {
        }
    }

    internal class MockFeedFixtureContext : IFeedFixtureContext
    {
        public MockFeedFixtureContext(MockFixtureContext execContext)
        {
            this.ExecContext = execContext;
        }

        public IFixtureContext ExecContext { get; private set; }
        public IPluginFeedProvider Feed { get; set; }

        public void Add(IFeedFixture subscriber)
        {

        }

        public void Remove(IFeedFixture subscriber)
        {

        }
    }
}
