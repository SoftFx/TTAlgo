using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface ITradeApi
    {
        Task<TradeResultInfo> OpenOrder(bool isAysnc, OpenOrderRequest request);
        Task<TradeResultInfo> CancelOrder(bool isAysnc, CancelOrderRequest request);
        Task<TradeResultInfo> ModifyOrder(bool isAysnc, ModifyOrderRequest request);
        Task<TradeResultInfo> CloseOrder(bool isAysnc, CloseOrderRequest request);
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
        IAsyncPagedEnumerator<TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, TradeHistoryRequestOptions options);
    }

    public interface IAsyncPagedEnumerator<T> : IDisposable
    {
        Task<T[]> GetNextPage();
    }


    public interface IFeedStorage
    {
        void Start();
        void Stop();
    }

    public interface ITickStorage : IFeedStorage
    {
        IEnumerable<QuoteInfo> GetQuoteStream();
    }

    public interface IBarStorage : IFeedStorage
    {
        IEnumerable<BarData> GetBarStream();
    }
}
