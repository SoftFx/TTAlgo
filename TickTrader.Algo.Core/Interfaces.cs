using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;

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

    public interface IPluginSetupTarget
    {
        void SetParameter(string id, object value);
        T GetFeedStrategy<T>() where T : FeedStrategy;
        void MapInput(string inputName, string symbolCode, Mapping mapping);
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
        void SendOpenOrder(ICallback<OrderCmdResultCodes> callback, OpenOrderRequest request);
        void SendCancelOrder(ICallback<OrderCmdResultCodes> callback, CancelOrderRequest request);
        void SendModifyOrder(ICallback<OrderCmdResultCodes> callback, ReplaceOrderRequest request);
        void SendCloseOrder(ICallback<OrderCmdResultCodes> callback, CloseOrderRequest request);
    }


    public interface ITradeHistoryProvider
    {
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(ThQueryOptions options);
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(int count, ThQueryOptions options);
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(DateTime from, DateTime to, ThQueryOptions options);
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(DateTime to, ThQueryOptions options);
    }

    public interface IAsyncCrossDomainEnumerator<T> : IDisposable
    {
        void GetNextPage(CrossDomainTaskProxy<T[]> pageCallback);
    }
}
