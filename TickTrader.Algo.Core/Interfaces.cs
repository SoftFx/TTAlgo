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
        void OpenOrder(TaskProxy<OpenModifyResult> waitHandler, string symbol, OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment, OrderExecOptions options, string tag);
        void CancelOrder(TaskProxy<CancelResult> waitHandler, string orderId, string clientOrderId, OrderSide side);
        void ModifyOrder(TaskProxy<OpenModifyResult> waitHandler, string orderId, string clientOrderId, string symbol, OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment);
        void CloseOrder(TaskProxy<CloseResult> waitHandler, string orderId, double? volume);
    }

    [Serializable]
    public struct OpenModifyResult
    {
        public OpenModifyResult(OrderCmdResultCodes code, OrderEntity order)
        {
            this.ResultCode = code;
            this.NewOrder = order;
        }

        public OrderCmdResultCodes ResultCode { get; private set; }
        public OrderEntity NewOrder { get; private set; }
    }

    [Serializable]
    public struct CancelResult
    {
        public CancelResult(OrderCmdResultCodes code)
        {
            this.ResultCode = code;
        }

        public OrderCmdResultCodes ResultCode { get; private set; }
    }

    [Serializable]
    public struct CloseResult
    {
        public CloseResult(OrderCmdResultCodes code, double execPrice = double.NaN, double execVolume = 0)
        {
            this.ResultCode = code;
            this.ExecPrice = execPrice;
            this.ExecVolume = execVolume;
        }

        public OrderCmdResultCodes ResultCode { get; private set; }
        public double ExecPrice { get; private set; }
        public double ExecVolume { get; private set; }
    }
}
