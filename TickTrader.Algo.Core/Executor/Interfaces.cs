using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, int depth);
        List<QuoteEntity> QueryTicks(string symbolCode, int count, DateTime to, int depth);
        void Subscribe(Action<QuoteEntity[]> FeedUpdated);
        void Unsubscribe();
        void SetSymbolDepth(string symbolCode, int depth);
    }

    public interface ISynchronizationContext
    {
        void Invoke(Action action);
    }

    public interface IAccountInfoProvider
    {
        double Balance { get; }
        string BalanceCurrency { get; }
        Api.AccountTypes AccountType { get; }
        string Account { get; }
        int Leverage { get; }

        void SyncInvoke(Action action);

        List<OrderEntity> GetOrders();
        IEnumerable<PositionExecReport> GetPositions();
        IEnumerable<AssetEntity> GetAssets();
        event Action<OrderExecReport> OrderUpdated;
        event Action<PositionExecReport> PositionUpdated;
        event Action<BalanceOperationReport> BalanceUpdated;
    }

    internal interface IFixtureContext
    {
        PluginBuilder Builder { get; }
        string MainSymbolCode { get; }
        Api.TimeFrames TimeFrame { get; }
        IPluginLogger Logger { get; }
        void Enqueue(QuoteEntity update);
        void Enqueue(Action<PluginBuilder> action);
        IPluginFeedProvider FeedProvider { get; }
        SubscriptionManager Dispenser { get; }
        FeedBufferStrategy BufferingStrategy { get; }
        //void Subscribe(IRateSubscription subscriber);
        //void Unsubscribe(IRateSubscription subscriber);
        //void Subscribe(IAllRatesSubscription subscriber);
        //void Unsubscribe(IAllRatesSubscription subscriber);
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
