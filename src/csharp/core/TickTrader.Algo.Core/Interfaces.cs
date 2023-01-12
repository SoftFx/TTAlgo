using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface ITradeApi
    {
        Task<TradeResultInfo> OpenOrder(bool isAsync, OpenOrderRequest request);
        Task<TradeResultInfo> CancelOrder(bool isAsync, CancelOrderRequest request);
        Task<TradeResultInfo> ModifyOrder(bool isAsync, ModifyOrderRequest request);
        Task<TradeResultInfo> CloseOrder(bool isAsync, CloseOrderRequest request);
    }

    public interface ITradeExecutor
    {
        void SendOpenOrder(OpenOrderRequest request);
        void SendCancelOrder(CancelOrderRequest request);
        void SendModifyOrder(ModifyOrderRequest request);
        void SendCloseOrder(CloseOrderRequest request);
    }


    public interface ITradeHistoryProvider
    {
        IAsyncPagedEnumerator<TradeReportInfo> GetTradeHistory(UtcTicks? from, UtcTicks? to, HistoryRequestOptions options);
        IAsyncPagedEnumerator<TriggerReportInfo> GetTriggerHistory(UtcTicks? from, UtcTicks? to, HistoryRequestOptions options);
    }

    public interface IAsyncPagedEnumerator<T> : IDisposable
    {
        Task<List<T>> GetNextPage();
    }
}
