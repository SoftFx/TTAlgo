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
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(ThQueryOptions options);
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(DateTime from, DateTime to, ThQueryOptions options);
        IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(DateTime to, ThQueryOptions options);
    }

    public interface IAsyncCrossDomainEnumerator<T> : IDisposable
    {
        void GetNextPage(CrossDomainTaskProxy<T[]> pageCallback);
    }
}
