using System;
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
        Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volume, double price, double? sl, double? tp, string comment, OrderExecOptions options, string tag);
        Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId, OrderSide side);
        Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, string symbol, OrderType type, OrderSide side, double currentVolume, double price, double? sl, double? tp, string comment);
        Task<OrderCmdResult> CloseOrder(bool isAysnc, string orderId, double? volume);
        Task<OrderCmdResult> CloseOrderBy(bool isAysnc, string orderId, string byOrderId);
    }

    public interface ITradeExecutor
    {
        void SendOpenOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId,
            string symbol, OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment, OrderExecOptions options, string tag);

        void SendCancelOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId,
            string orderId, OrderSide side);

        void SendModifyOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId,
            string orderId, string symbol, OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment);

        void SendCloseOrder(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId,
            string orderId, double? volume);

        void SendCloseOrderBy(CrossDomainCallback<OrderCmdResultCodes> callback, string operationId,
            string orderId, string byOrderId);
    }

    //[Serializable]
    //public struct OpenModifyResult
    //{
    //    public OpenModifyResult(OrderCmdResultCodes code, OrderEntity order)
    //    {
    //        this.ResultCode = code;
    //        this.NewOrder = order;
    //    }

    //    public OrderCmdResultCodes ResultCode { get; private set; }
    //    public OrderEntity NewOrder { get; private set; }
    //}

    //[Serializable]
    //public struct CancelResult
    //{
    //    public CancelResult(OrderCmdResultCodes code)
    //    {
    //        this.ResultCode = code;
    //    }

    //    public OrderCmdResultCodes ResultCode { get; private set; }
    //}

    //[Serializable]
    //public struct CloseResult
    //{
    //    public CloseResult(OrderCmdResultCodes code, double execPrice = double.NaN, double execVolume = 0)
    //    {
    //        this.ResultCode = code;
    //        this.ExecPrice = execPrice;
    //        this.ExecVolume = execVolume;
    //    }

    //    public OrderCmdResultCodes ResultCode { get; private set; }
    //    public double ExecPrice { get; private set; }
    //    public double ExecVolume { get; private set; }
    //}
}
