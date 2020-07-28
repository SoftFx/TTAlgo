using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.Algo.Api;
using ActorSharp;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model
{
    internal interface IServerInterop
    {
        Task<ConnectionErrorInfo> Connect(string address, string login, string password, CancellationToken cancelToken);
        Task Disconnect();

        IFeedServerApi FeedApi { get; }
        ITradeServerApi TradeApi { get; }

        event Action<IServerInterop, ConnectionErrorInfo> Disconnected;
    }

    internal interface ITradeServerApi
    {
        bool AutoAccountInfo { get; }

        Task<Domain.AccountInfo> GetAccountInfo();
        Task<Domain.PositionInfo[]> GetPositions();

        void GetTradeRecords(BlockingChannel<Domain.OrderInfo> rxStream);
        void GetTradeHistory(BlockingChannel<Domain.TradeReportInfo> rxStream, DateTime? from, DateTime? to, bool skipCancelOrders, bool backwards);

        event Action<Domain.PositionExecReport> PositionReport;
        event Action<ExecutionReport> ExecutionReport;
        //event Action<AccountInfoEventArgs> AccountInfoReceived;
        event Action<Domain.TradeReportInfo> TradeTransactionReport;
        event Action<Domain.BalanceOperation> BalanceOperation;

        Task<OrderInteropResult> SendModifyOrder(Domain.ModifyOrderRequest request);
        Task<OrderInteropResult> SendCloseOrder(Domain.CloseOrderRequest request);
        Task<OrderInteropResult> SendCancelOrder(Domain.CancelOrderRequest request);
        Task<OrderInteropResult> SendOpenOrder(Domain.OpenOrderRequest request);
    }

    internal interface IFeedServerApi
    {
        bool AutoSymbols { get; }

        event Action<QuoteEntity> Tick;
        event Action<Domain.SymbolInfo[]> SymbolInfo;
        event Action<CurrencyEntity[]> CurrencyInfo;

        Task<Domain.CurrencyInfo[]> GetCurrencies();
        Task<Domain.SymbolInfo[]> GetSymbols();
        Task<QuoteEntity[]> SubscribeToQuotes(string[] symbols, int depth);
        Task<QuoteEntity[]> GetQuoteSnapshot(string[] symbols, int depth);
        void DownloadBars(BlockingChannel<BarEntity> stream, string symbol, DateTime from, DateTime to, BarPriceType priceType, TimeFrames barPeriod);
        Task<BarEntity[]> DownloadBarPage(string symbol, DateTime from, int count, BarPriceType priceType, TimeFrames barPeriod);
        void DownloadQuotes(BlockingChannel<QuoteEntity> stream, string symbol, DateTime from, DateTime to, bool includeLevel2);
        Task<QuoteEntity[]> DownloadQuotePage(string symbol, DateTime from, int count, bool includeLevel2);
        Task<Tuple<DateTime?, DateTime?>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame);
    }
}
