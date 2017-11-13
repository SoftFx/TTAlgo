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
        Task<ConnectionErrorCodes> Connect(string address, string login, string password, CancellationToken cancelToken);
        Task Disconnect();

        IFeedServerApi FeedApi { get; }
        ITradeServerApi TradeApi { get; }

        event Action<IServerInterop, ConnectionErrorCodes> Disconnected;
    }

    public interface ITradeServerApi : ITradeExecutor
    {
        Task<AccountEntity> GetAccountInfo();
        Task<OrderEntity[]> GetTradeRecords();
        Task<PositionEntity[]> GetPositions();

        Task<OrderCmdResult> OpenOrder(OpenOrderRequest request);
        Task<OrderCmdResult> CancelOrder(CancelOrderRequest request);
        Task<OrderCmdResult> ModifyOrder(ReplaceOrderRequest request);
        Task<OrderCmdResult> CloseOrder(CloseOrderRequest request);

        IAsyncEnumerator<TradeReportEntity[]> GetTradeHistory(DateTime? from, DateTime? to, bool skipCancelOrders);

        event Action<PositionEntity> PositionReport;
        event Action<ExecutionReport> ExecutionReport;
        //event Action<AccountInfoEventArgs> AccountInfoReceived;
        event Action<TradeReportEntity> TradeTransactionReport;
        event Action<BalanceOperationReport> BalanceOperation;
    }

    public interface IFeedServerApi
    {
        event Action<QuoteEntity> Tick;

        Task<CurrencyEntity[]> GetCurrencies();
        Task<SymbolEntity[]> GetSymbols();
        Task SubscribeToQuotes(string[] symbols, int depth);
        IAsyncEnumerator<BarEntity[]> GetHistoryBars(string symbol, DateTime from, DateTime to, BarPriceType priceType, TimeFrames barPeriod);
        Task<List<BarEntity>> GetHistoryBars(string symbol, DateTime startTime, int count, BarPriceType priceType, TimeFrames barPeriod);
        Task<HistoryFilesPackage> DownloadTickFiles(string symbol, DateTime refTimePoint, bool includeLevel2);
    }
}
