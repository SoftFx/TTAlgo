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
    }

    public interface IPluginFeedProvider
    {
        ISynchronizationContext Sync { get; }
        IEnumerable<QuoteEntity> GetSnapshot();
        List<BarEntity> QueryBars(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, int depth);
        void Subscribe(Action<QuoteEntity[]> FeedUpdated);
        void Unsubscribe();
        void SetSymbolDepth(string symbolCode, int depth);
    }

    public interface IBarBasedFeed : IPluginFeedProvider
    {
        List<BarEntity> GetMainSeries();
    }

    public interface IQuoteBasedFeed : IPluginFeedProvider
    {
        List<QuoteEntity> GetMainSeries();
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

        void SyncInvoke(Action action);

        List<OrderEntity> GetOrders();
        IEnumerable<OrderEntity> GetPosition();
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
        DateTime TimePeriodStart { get; }
        DateTime TimePeriodEnd { get; }
        IPluginLogger Logger { get; }
        void Enqueue(QuoteEntity update);
        void Enqueue(Action<PluginBuilder> action);

        //IEnumerable<BarEntity> QueryBars(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        //IEnumerable<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to);
        //IEnumerable<QuoteEntityL2> QueryTicksL2(string symbolCode, DateTime from, DateTime to);
        //void Add(IFeedFixture subscriber);
        //void Remove(IFeedFixture subscriber);
        //void Subscribe(string symbolCode, int depth);
        //void Unsubscribe(string symbolCode);
        //void InvokeUpdateOnCustomSubscription(QuoteEntity update);
    }

    internal interface IFeedFixtureContext
    {
        IFixtureContext ExecContext { get; }
        IPluginFeedProvider Feed { get; }
        void Add(IFeedFixture subscriber);
        void Remove(IFeedFixture subscriber);
    }


    //public interface IPluginInvoker
    //{
    //    void StartBatch();
    //    void StopBatch();
    //    void InvokeInit();
    //    void IncreaseVirtualPosition();
    //    void InvokeOnStart();
    //    void InvokeOnStop();
    //    void InvokeCalculate(bool isUpdate);
    //    void InvokeOnQuote(QuoteEntity quote);
    //}

    //public interface IInvokeStrategyContext
    //{
    //    IPluginInvoker Builder { get; }
    //    BufferUpdateResults UpdateBuffers(FeedUpdate update);
    //    void InvokeFeedEvents(FeedUpdate update);
    //}

    internal interface IFeedFixture 
    {
        string SymbolCode { get; }
        int Depth { get; }
        //void OnBufferUpdated(Quote quote); // called always
        void OnUpdateEvent(Quote quote); // events may be skipped by latency filter or optimizer
    }

    public interface ITimeRef
    {
        DateTime? GetTimeAtIndex(int index);
    }
}
