using Google.Protobuf;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    //public enum BufferUpdateResults { Extended = 2, LastItemUpdated = 1, NotUpdated = 0 }

    public struct BufferUpdateResult
    {
        /// <summary>
        /// True if last bar was updated. May be false event in case ExtendedBy > 0
        /// </summary>
        public bool IsLastUpdated { get; set; }
        public int ExtendedBy { get; set; }

        public static BufferUpdateResult operator +(BufferUpdateResult x, BufferUpdateResult y)
        {
            return new BufferUpdateResult()
            {
                IsLastUpdated = x.IsLastUpdated || y.IsLastUpdated,
                ExtendedBy = Math.Max(x.ExtendedBy, y.ExtendedBy)
            };
        }
    }

    public interface IPluginMetadata
    {
        IEnumerable<Domain.SymbolInfo> GetSymbolMetadata();
        IEnumerable<CurrencyEntity> GetCurrencyMetadata();
    }

    public interface IFeedHistoryProvider
    {
        List<BarEntity> QueryBars(string symbolCode, BarPriceType priceType, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        List<BarEntity> QueryBars(string symbolCode, BarPriceType priceType, DateTime from, int size, Api.TimeFrames timeFrame);
        List<QuoteInfo> QueryTicks(string symbolCode, DateTime from, DateTime to, bool level2);
        List<QuoteInfo> QueryTicks(string symbolCode, DateTime from, int count, bool level2);
    }

    public interface IFeedSubscription
    {
        List<QuoteInfo> Modify(List<FeedSubscriptionUpdate> updates);
        void CancelAll();
    }

    public interface IFeedProvider : IFeedSubscription
    {
        ISyncContext Sync { get; }
        List<QuoteInfo> GetSnapshot();
        
        event Action<QuoteInfo> RateUpdated;
        event Action<List<QuoteInfo>> RatesUpdated;
    }

    public interface ILinkOutput<T> : IDisposable
    {
        event Action<T> MsgReceived;
    }

    public interface IAccountInfoProvider
    {
        void SyncInvoke(Action action);

        Domain.AccountInfo GetAccountInfo();
        List<Domain.OrderInfo> GetOrders();
        List<Domain.PositionInfo> GetPositions();

        event Action<Domain.OrderExecReport> OrderUpdated;
        event Action<Domain.PositionExecReport> PositionUpdated;
        event Action<Domain.BalanceOperation> BalanceUpdated;
    }

    internal interface IFixtureContext
    {
        string InstanceId { get; }
        PluginBuilder Builder { get; }
        AlgoMarketState MarketData { get; }
        FeedStrategy FeedStrategy { get; }
        string MainSymbolCode { get; }
        Api.TimeFrames TimeFrame { get; }
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

        //void Subscribe(IRateSubscription subscriber);
        //void Unsubscribe(IRateSubscription subscriber);
        //void Subscribe(IAllRatesSubscription subscriber);
        //void Unsubscribe(IAllRatesSubscription subscriber);
    }

    internal interface IExecutorFixture : IDisposable
    {
        void Start();
        void Stop();
        void Restart();
    }

    internal class NullExecFixture : IExecutorFixture
    {
        public void Dispose() { }
        public void Restart() { }
        public void Start() { }
        public void Stop() { }
    }

    //internal interface IFeedFixtureContext
    //{
    //    IFixtureContext ExecContext { get; }

    //    //void Add(IRateSubscription subscriber);
    //    //void Remove(IRateSubscription subscriber);
    //}

    //internal interface IRateSubscription 
    //{
    //    string SymbolCode { get; }
    //    int Depth { get; }
    //    void OnUpdateEvent(Quote quote); // events may be skipped by latency filter or optimizer
    //}

    //internal interface IAllRatesSubscription
    //{
    //    void OnUpdateEvent(Quote quote); // events may be skipped by latency filter or optimizer
    //}

    public interface ITimeRef
    {
        int LastIndex { get; }
        DateTime this[int index] { get; }
        event Action Appended;
    }
}
