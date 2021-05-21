using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public interface IPluginPermissions
    {
        bool TradeAllowed { get; }

        bool Isolated { get; }
    }

    public interface IPluginSubscriptionHandler
    {
        void Subscribe(string smbCode, int depth);
        void Unsubscribe(string smbCode);
    }

    public interface ITradeApi
    {
        Task<Domain.TradeResultInfo> OpenOrder(bool isAysnc, Domain.OpenOrderRequest request);
        Task<Domain.TradeResultInfo> CancelOrder(bool isAysnc, Domain.CancelOrderRequest request);
        Task<Domain.TradeResultInfo> ModifyOrder(bool isAysnc, Domain.ModifyOrderRequest request);
        Task<Domain.TradeResultInfo> CloseOrder(bool isAysnc, Domain.CloseOrderRequest request);
    }

    public interface ITradeExecutor
    {
        void SendOpenOrder(Domain.OpenOrderRequest request);
        void SendCancelOrder(Domain.CancelOrderRequest request);
        void SendModifyOrder(Domain.ModifyOrderRequest request);
        void SendCloseOrder(Domain.CloseOrderRequest request);
    }


    public interface ITradeHistoryProvider
    {
        IAsyncPagedEnumerator<Domain.TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, Domain.TradeHistoryRequestOptions options);
    }

    public interface IAsyncPagedEnumerator<T> : IDisposable
    {
        Task<T[]> GetNextPage();
    }
}
