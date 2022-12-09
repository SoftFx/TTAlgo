using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public struct BufferUpdateResult
    {
        /// <summary>
        /// True if last bar was updated. May be false event in case ExtendedBy > 0
        /// </summary>
        public bool IsLastUpdated { get; set; }
        public int ExtendedBy { get; set; }

        public BufferUpdateResult(bool isLastUpdated, int extendBy)
        {
            IsLastUpdated = isLastUpdated;
            ExtendedBy = extendBy;
        }

        public override string ToString()
        {
            return $"{IsLastUpdated}; {ExtendedBy}";
        }

        public static BufferUpdateResult Combine(BufferUpdateResult x, BufferUpdateResult y)
        {
            return new BufferUpdateResult(x.IsLastUpdated || y.IsLastUpdated, Math.Max(x.ExtendedBy, y.ExtendedBy));
        }

        public static BufferUpdateResult operator +(BufferUpdateResult x, BufferUpdateResult y)
        {
            return new BufferUpdateResult(x.IsLastUpdated || y.IsLastUpdated, x.ExtendedBy + y.ExtendedBy);
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
        List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, UtcTicks to);
        List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, int count);
        List<QuoteInfo> QueryQuotes(string symbol, UtcTicks from, UtcTicks to, bool level2);
        List<QuoteInfo> QueryQuotes(string symbol, UtcTicks from, int count, bool level2);

        Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, UtcTicks to);
        Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, UtcTicks from, int count);
        Task<List<QuoteInfo>> QueryQuotesAsync(string symbol, UtcTicks from, UtcTicks to, bool level2);
        Task<List<QuoteInfo>> QueryQuotesAsync(string symbol, UtcTicks from, int count, bool level2);
    }

    public interface IFeedProvider
    {
        List<QuoteInfo> GetQuoteSnapshot();
        Task<List<QuoteInfo>> GetQuoteSnapshotAsync();
        IQuoteSub GetQuoteSub();
        IBarSub GetBarSub();

        event Action<QuoteInfo> QuoteUpdated;
        event Action<BarUpdate> BarUpdated;
    }

    public interface IAccountInfoProvider
    {
        AccountInfo GetAccountInfo();
        List<OrderInfo> GetOrders();
        List<PositionInfo> GetPositions();

        event Action<OrderExecReport> OrderUpdated;
        event Action<PositionExecReport> PositionUpdated;
        event Action<BalanceOperation> BalanceUpdated;
    }
}
