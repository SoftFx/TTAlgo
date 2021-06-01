using System;
using System.Threading;
using System.Threading.Tasks;
using ActorSharp;
using TickTrader.Algo.Domain;
using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Account
{
    public interface IServerInterop
    {
        Task<ConnectionErrorInfo> Connect(string address, string login, string password, CancellationToken cancelToken);
        Task Disconnect();

        IFeedServerApi FeedApi { get; }
        ITradeServerApi TradeApi { get; }

        event Action<IServerInterop, ConnectionErrorInfo> Disconnected;
    }

    public delegate IServerInterop ServerInteropFactory(ConnectionOptions options, int loggerId);

    public interface ITradeServerApi
    {
        bool AutoAccountInfo { get; }

        Task<AccountInfo> GetAccountInfo();
        Task<PositionInfo[]> GetPositions();

        void GetTradeRecords(BlockingChannel<OrderInfo> rxStream);
        void GetTradeHistory(BlockingChannel<TradeReportInfo> rxStream, DateTime? from, DateTime? to, bool skipCancelOrders, bool backwards);

        event Action<PositionExecReport> PositionReport;
        event Action<ExecutionReport> ExecutionReport;
        event Action<TradeReportInfo> TradeTransactionReport;
        event Action<BalanceOperation> BalanceOperation;

        Task<OrderInteropResult> SendModifyOrder(ModifyOrderRequest request);
        Task<OrderInteropResult> SendCloseOrder(CloseOrderRequest request);
        Task<OrderInteropResult> SendCancelOrder(CancelOrderRequest request);
        Task<OrderInteropResult> SendOpenOrder(OpenOrderRequest request);
    }

    public interface IFeedServerApi
    {
        bool AutoSymbols { get; }

        event Action<QuoteInfo> Tick;
        event Action<SymbolInfo[]> SymbolInfo;
        event Action<CurrencyInfo[]> CurrencyInfo;

        Task<CurrencyInfo[]> GetCurrencies();
        Task<SymbolInfo[]> GetSymbols();
        Task<QuoteInfo[]> SubscribeToQuotes(string[] symbols, int depth);
        Task<QuoteInfo[]> GetQuoteSnapshot(string[] symbols, int depth);
        void DownloadBars(BlockingChannel<BarData> stream, string symbol, Timestamp from, Timestamp to, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe);
        Task<BarData[]> DownloadBarPage(string symbol, Timestamp from, int count, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe);
        void DownloadQuotes(BlockingChannel<QuoteInfo> stream, string symbol, Timestamp from, Timestamp to, bool includeLevel2);
        Task<QuoteInfo[]> DownloadQuotePage(string symbol, Timestamp from, int count, bool includeLevel2);
        Task<Tuple<DateTime?, DateTime?>> GetAvailableRange(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe);
    }
}
