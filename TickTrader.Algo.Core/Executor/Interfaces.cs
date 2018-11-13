using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

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
            bool isLastUpdated = x.IsLastUpdated || (x.ExtendedBy == 0 && y.IsLastUpdated);

            return new BufferUpdateResult()
            {
                IsLastUpdated = isLastUpdated,
                ExtendedBy = x.ExtendedBy + y.ExtendedBy
            };
        }
    }

    public interface IPluginMetadata
    {
        IEnumerable<SymbolEntity> GetSymbolMetadata();
        IEnumerable<CurrencyEntity> GetCurrencyMetadata();
    }

    public interface IPluginFeedProvider
    {
        ISynchronizationContext Sync { get; }
        IEnumerable<QuoteEntity> GetSnapshot();
        List<BarEntity> QueryBars(string symbolCode, BarPriceType priceType, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        List<BarEntity> QueryBars(string symbolCode, BarPriceType priceType, DateTime from, int size, Api.TimeFrames timeFrame);
        List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, bool level2);
        List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, int count, bool level2);
        void Subscribe(Action<QuoteEntity[]> FeedUpdated);
        void Unsubscribe();
        void SetSymbolDepth(string symbolCode, int depth);
    }

    public interface ISynchronizationContext
    {
        void Invoke(Action action);
    }

    public interface ILinkOutput<T> : IDisposable
    {
        event Action<T> MsgReceived;
    }

    public interface IAccountInfoProvider
    {
        AccountEntity AccountInfo { get; }

        void SyncInvoke(Action action);

        List<OrderEntity> GetOrders();
        IEnumerable<PositionExecReport> GetPositions();
        event Action<OrderExecReport> OrderUpdated;
        //event Action<PositionExecReport> PositionUpdated;
        event Action<BalanceOperationReport> BalanceUpdated;
    }

    internal interface IFixtureContext
    {
        PluginBuilder Builder { get; }
        string MainSymbolCode { get; }
        Api.TimeFrames TimeFrame { get; }
        PluginLoggerAdapter Logger { get; }

        void EnqueueQuote(QuoteEntity update);
        void EnqueueTradeUpdate(Action<PluginBuilder> action);
        void EnqueueEvent(Action<PluginBuilder> action);
        void EnqueueCustomInvoke(Action<PluginBuilder> action);
        void ProcessNextOrderUpdate();
        void OnInternalException(Exception ex);

        IPluginFeedProvider FeedProvider { get; }
        SubscriptionManager Dispenser { get; }
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
