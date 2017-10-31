using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SoftFX.Extended;
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
        AccountEntity AccountInfo { get; }
        IEnumerable<OrderEntity> TradeRecords { get; }
        IEnumerable<PositionEntity> Positions { get; }

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
        CurrencyEntity[] Currencies { get; }
        SymbolEntity[] Symbols { get; }
        void SubscribeToQuotes(string[] symbols, int depth);

        event Action<QuoteEntity> Tick;

        BarHistoryReport GetHistoryBars(string symbol, DateTime startTime, int count, PriceType priceType, BarPeriod barPeriod);
        Task<HistoryFilesPackage> DownloadTickFiles(string symbol, DateTime refTimePoint, bool includeLevel2);
    }
}
