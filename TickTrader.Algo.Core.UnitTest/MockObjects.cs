﻿using Google.Protobuf;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Domain;

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

        internal static QuoteInfo CreateQuote(string timestamp, double bid, double? ask)
        {
            return CreateQuote("", DateTime.Parse(timestamp), bid, ask);
        }

        internal static QuoteInfo CreateQuote(string symbol, DateTime timestamp, double bid, double? ask = null)
        {
            return new QuoteInfo(symbol, timestamp, bid, ask ?? bid);
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
        private SubscriptionFixtureManager dispenser;
        private FeedBufferStrategy bStrategy;
        private MarketStateFixture _marketState;

        public MockFixtureContext()
        {
            _marketState = new MarketStateFixture(this);
            dispenser = new SubscriptionFixtureManager(this, _marketState);
            Builder = new PluginBuilder(new Metadata.PluginMetadata(typeof(MockBot)));
            Builder.Logger = new NullLogger();
            bStrategy = new TimeSpanStrategy(TimePeriodStart, TimePeriodEnd);
        }

        public string InstanceId => "Mock Plugin";
        public PluginBuilder Builder { get; private set; }
        public PluginLoggerAdapter Logger => Builder.LogAdapter;
        public string MainSymbolCode { get; set; }
        public TimeFrames TimeFrame { get; set; }
        public DateTime TimePeriodEnd { get; set; }
        public DateTime TimePeriodStart { get; set; }
        public IFeedProvider FeedProvider { get; set; }
        public IFeedHistoryProvider FeedHistory { get; set; }
        public FeedBufferStrategy BufferingStrategy => bStrategy;
        public SubscriptionFixtureManager Dispenser => dispenser;
        public AlgoMarketState MarketData { get; } = new AlgoMarketState();
        public FeedStrategy FeedStrategy => throw new NotImplementedException();
        public IAccountInfoProvider AccInfoProvider => throw new NotImplementedException();
        public ITradeExecutor TradeExecutor => throw new NotImplementedException();
        public bool IsGlobalUpdateMarshalingEnabled => false;

        public void EnqueueTradeUpdate(Action<PluginBuilder> action)
        {
        }

        public void EnqueueQuote(QuoteInfo update)
        {
        }

        public void EnqueueEvent(Action<PluginBuilder> action)
        {
        }

        public void EnqueueCustomInvoke(Action<PluginBuilder> action)
        {
        }

        public void ProcessNextOrderUpdate()
        {
        }

        public void OnInternalException(Exception ex)
        {
        }

        public void EnqueueUserCallback(Action<PluginBuilder> action)
        {
            throw new NotImplementedException();
        }

        public void SendExtUpdate(object update)
        {
        }

        public void SendNotification(IMessage msg)
        {
        }
    }
}
