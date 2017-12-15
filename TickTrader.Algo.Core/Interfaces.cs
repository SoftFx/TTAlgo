using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public interface ITradePermissions
    {
        bool TradeAllowed { get; }
    }

    public interface IPluginSubscriptionHandler
    {
        void Subscribe(string smbCode, int depth);
        void Unsubscribe(string smbCode);
    }

    public interface IPluginSetupTarget
    {
        void SetParameter(string id, object value);
        T GetFeedStrategy<T>() where T : FeedStrategy;
    }

    public interface ITradeApi
    {
        Task<TradeResultEntity> OpenOrder(bool isAysnc, OpenOrderRequest request);
        Task<TradeResultEntity> CancelOrder(bool isAysnc, CancelOrderRequest request);
        Task<TradeResultEntity> ModifyOrder(bool isAysnc, ReplaceOrderRequest request);
        Task<TradeResultEntity> CloseOrder(bool isAysnc, CloseOrderRequest request);
    }

    public interface ITradeExecutor
    {
        void SendOpenOrder(CrossDomainCallback<OrderCmdResultCodes> callback, OpenOrderRequest request);
        void SendCancelOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CancelOrderRequest request);
        void SendModifyOrder(CrossDomainCallback<OrderCmdResultCodes> callback, ReplaceOrderRequest request);
        void SendCloseOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CloseOrderRequest request);
    }


    public interface ITradeHistoryProvider
    {
        IAsyncCrossDomainEnumerator<TradeReport> GetTradeHistory(bool skipCancelOrders);
        IAsyncCrossDomainEnumerator<TradeReport> GetTradeHistory(DateTime from, DateTime to, bool skipCancelOrders);
        IAsyncCrossDomainEnumerator<TradeReport> GetTradeHistory(DateTime to, bool skipCancelOrders);
    }

    public interface IAsyncCrossDomainEnumerator<T> : IDisposable
    {
        void GetNextPage(CrossDomainTaskProxy<T[]> pageCallback);
    }
}
