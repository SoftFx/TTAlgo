﻿using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        IEnumerable<Domain.CurrencyInfo> GetCurrencyMetadata();
    }

    public interface IFeedHistoryProvider
    {
        List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to);
        List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count);
        List<QuoteInfo> QueryQuotes(string symbol, Timestamp from, Timestamp to, bool level2);
        List<QuoteInfo> QueryQuotes(string symbol, Timestamp from, int count, bool level2);

        Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to);
        Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count);
        Task<List<QuoteInfo>> QueryQuotesAsync(string symbol, Timestamp from, Timestamp to, bool level2);
        Task<List<QuoteInfo>> QueryQuotesAsync(string symbol, Timestamp from, int count, bool level2);
    }

    public interface IFeedSubscription
    {
        List<QuoteInfo> Modify(List<FeedSubscriptionUpdate> updates);
        Task<List<QuoteInfo>> ModifyAsync(List<FeedSubscriptionUpdate> updates);
        void CancelAll();
        Task CancelAllAsync();
    }

    public interface IFeedProvider : IFeedSubscription
    {
        ISyncContext Sync { get; }
        List<QuoteInfo> GetSnapshot();
        Task<List<QuoteInfo>> GetSnapshotAsync();

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

        //void Subscribe(IRateSubscription subscriber);
        //void Unsubscribe(IRateSubscription subscriber);
        //void Subscribe(IAllRatesSubscription subscriber);
        //void Unsubscribe(IAllRatesSubscription subscriber);
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
        Timestamp this[int index] { get; }
        event Action Appended;
    }
}
