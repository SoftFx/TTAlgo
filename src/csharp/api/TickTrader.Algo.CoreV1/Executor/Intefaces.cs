using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal interface IFixtureContext
    {
        string InstanceId { get; }
        PluginBuilder Builder { get; }
        AlgoMarketState MarketData { get; }
        FeedStrategy FeedStrategy { get; }
        string MainSymbolCode { get; }
        Feed.Types.Timeframe TimeFrame { get; }
        Feed.Types.Timeframe ModelTimeFrame { get; }
        PluginLoggerAdapter Logger { get; }
        bool IsGlobalUpdateMarshalingEnabled { get; }

        void EnqueueQuote(QuoteInfo update);
        void EnqueueTradeUpdate(Action<PluginBuilder> action);
        void EnqueueEvent(Action<PluginBuilder> action);
        void EnqueueCustomInvoke(Action<PluginBuilder> action);
        void ProcessNextOrderUpdate();
        void OnInternalException(Exception ex);

        void SendExtUpdate(object update);
        void SendNotification(IMessage message);

        IFeedProvider FeedProvider { get; }
        IFeedHistoryProvider FeedHistory { get; }
        SubscriptionFixtureManager Dispenser { get; }
        FeedBufferStrategy BufferingStrategy { get; }
        IAccountInfoProvider AccInfoProvider { get; }
        ITradeExecutor TradeExecutor { get; }
    }

    internal interface IExecutorFixture : IDisposable
    {
        void Start();
        void Stop();
        void PreRestart();
        void PostRestart();
    }

    internal class NullExecFixture : IExecutorFixture
    {
        public void Dispose() { }
        public void PreRestart() { }
        public void PostRestart() { }
        public void Start() { }
        public void Stop() { }
    }

    public interface ITimeRef
    {
        int LastIndex { get; }
        long this[int index] { get; }
        event Action Appended;
    }

    public interface IPluginSetupTarget
    {
        void SetParameter(string id, object value);
        T GetFeedStrategy<T>() where T : FeedStrategy;
        void SetupOutput<T>(string id, bool enabled);
    }
}
