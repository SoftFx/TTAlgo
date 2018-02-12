using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model
{
    public interface IServerInterop
    {
        Task<ConnectionErrorInfo> Connect(string address, string login, string password, CancellationToken cancelToken);
        Task Disconnect();

        IFeedServerApi FeedApi { get; }
        ITradeServerApi TradeApi { get; }

        event Action<IServerInterop, ConnectionErrorInfo> Disconnected;
    }

    public interface ITradeServerApi
    {
        Task<AccountEntity> GetAccountInfo();
        Task<OrderEntity[]> GetTradeRecords();
        Task<PositionEntity[]> GetPositions();
        void AllowTradeRequests();
        void DenyTradeRequests();

        IAsyncEnumerator<TradeReportEntity[]> GetTradeHistory(DateTime? from, DateTime? to, bool skipCancelOrders);

        event Action<PositionEntity> PositionReport;
        event Action<ExecutionReport> ExecutionReport;
        //event Action<AccountInfoEventArgs> AccountInfoReceived;
        event Action<TradeReportEntity> TradeTransactionReport;
        event Action<BalanceOperationReport> BalanceOperation;

        Task<OrderCmdResultCodes> SendModifyOrder(ReplaceOrderRequest request);
        Task<OrderCmdResultCodes> SendCloseOrder(CloseOrderRequest request);
        Task<OrderCmdResultCodes> SendCancelOrder(CancelOrderRequest request);
        Task<OrderCmdResultCodes> SendOpenOrder(OpenOrderRequest request);
    }

    public interface IFeedServerApi
    {
        event Action<QuoteEntity> Tick;

        Task<CurrencyEntity[]> GetCurrencies();
        Task<SymbolEntity[]> GetSymbols();
        Task SubscribeToQuotes(string[] symbols, int depth);
        Task<QuoteEntity[]> GetQuoteSnapshot(string[] symbols, int depth);
        IAsyncEnumerator<Slice<BarEntity>> DownloadBars(string symbol, DateTime from, DateTime to, BarPriceType priceType, TimeFrames barPeriod);
        Task<BarEntity[]> DownloadBarPage(string symbol, DateTime from, int count, BarPriceType priceType, TimeFrames barPeriod);
        IAsyncEnumerator<Slice<QuoteEntity>> DownloadQuotes(string symbol, DateTime from, DateTime to, bool includeLevel2);
        Task<QuoteEntity[]> DownloadQuotePage(string symbol, DateTime from, int count, bool includeLevel2);
        Task<Tuple<DateTime, DateTime>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame);
    }
}
