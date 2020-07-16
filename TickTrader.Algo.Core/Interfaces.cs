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
        Task<TradeResultEntity> OpenOrder(bool isAysnc, OpenOrderCoreRequest request);
        Task<TradeResultEntity> CancelOrder(bool isAysnc, CancelOrderRequest request);
        Task<TradeResultEntity> ModifyOrder(bool isAysnc, ReplaceOrderCoreRequest request);
        Task<TradeResultEntity> CloseOrder(bool isAysnc, CloseOrderCoreRequest request);
    }

    public interface ITradeExecutor
    {
        void SendOpenOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, OpenOrderCoreRequest request);
        void SendCancelOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, CancelOrderRequest request);
        void SendModifyOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, ReplaceOrderCoreRequest request);
        void SendCloseOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, CloseOrderCoreRequest request);
    }


    public interface ITradeHistoryProvider
    {
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(ThQueryOptions options);
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(DateTime from, DateTime to, ThQueryOptions options);
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(DateTime to, ThQueryOptions options);
    }

    public interface IAsyncCrossDomainEnumerator<T> : IDisposable
    {
        void GetNextPage(CrossDomainTaskProxy<T[]> pageCallback);
    }
}
