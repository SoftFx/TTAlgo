using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Subscriptions;
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
        IEnumerable<SymbolInfo> GetSymbolMetadata();
        IEnumerable<CurrencyInfo> GetCurrencyMetadata();
        IEnumerable<FullQuoteInfo> GetLastQuoteMetadata();
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

    public interface IFeedProvider
    {
        ISyncContext Sync { get; }
        List<QuoteInfo> GetSnapshot();
        Task<List<QuoteInfo>> GetSnapshotAsync();
        IQuoteSub GetSubscription();

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

        AccountInfo GetAccountInfo();
        List<OrderInfo> GetOrders();
        List<PositionInfo> GetPositions();

        event Action<OrderExecReport> OrderUpdated;
        event Action<PositionExecReport> PositionUpdated;
        event Action<BalanceOperation> BalanceUpdated;
    }
}
