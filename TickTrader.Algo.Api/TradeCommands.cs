using System;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface TradeCommands
    {
        Task<OrderCmdResult> OpenOrder(bool isAsync, OpenOrderRequest request);
        Task<OrderCmdResult> CancelOrder(bool isAsync, string orderId);
        Task<OrderCmdResult> ModifyOrder(bool isAsync, ModifyOrderRequest request);
        Task<OrderCmdResult> CloseOrder(bool isAsync, CloseOrderRequest request);
        Task<OrderCmdResult> CloseOrderBy(bool isAsync, string orderId, string byOrderId);
    }

    public interface OrderCmdResult
    {
        OrderCmdResultCodes ResultCode { get; }
        bool IsCompleted { get; }
        bool IsFaulted { get; }
        Order ResultingOrder { get; }
        DateTime TransactionTime { get; }
    }

    [Flags]
    public enum OrderExecOptions
    {
        None = 0,
        ImmediateOrCancel = 1,
        OneCancelsTheOther = 2,
    }

    [Flags]
    public enum OrderOptions
    {
        None = 0,
        ImmediateOrCancel = 1,
        MarketWithSlippage = 2,
        HiddenIceberg = 4,
        OneCancelsTheOther = 8,
    }

    public enum OrderCmdResultCodes
    {
        Ok = 0,
        UnknownError = 1,
        InternalError = 5,
        ConnectionError = 6,
        Timeout = 7,
        TradeServerError = 8,
        DealerReject = 100,
        Unsupported = 101,
        SymbolNotFound = 102,
        OrderNotFound = 103,
        IncorrectVolume = 104,
        OffQuotes = 105,
        IncorrectOrderId = 106,
        IncorrectPrice = 107,
        IncorrectTp = 108,
        IncorrectSl = 109,
        NotEnoughMoney = 110,
        TradeNotAllowed = 111,
        IncorrectStopPrice = 112,
        IncorrectMaxVisibleVolume = 113,
        IncorrectExpiration = 114,
        DealingTimeout = 115,
        Misconfiguration = 116,
        OrderLocked = 117,
        IncorrectPricePrecision = 118,
        CloseOnlyTrading = 119,
        MarketWithMaxVisibleVolume = 120,
        InvalidAmountChange = 121,
        CannotBeModified = 122,
        MaxVisibleVolumeNotSupported = 123,
        ReadOnlyAccount = 124,
        IncorrectSlippage = 125,
        IncorrectType = 126,
        ThrottlingError = 127,
        OCORelatedIdNotFound = 128,
        RelatedOrderAlreadyExistOCO = 129,
    }

    public static class OrderOptionExtension
    {
        public static OrderExecOptions ToExec(this OrderOptions options)
        {
            switch (options)
            {
                case OrderOptions.ImmediateOrCancel:
                    return OrderExecOptions.ImmediateOrCancel;

                default:
                    return OrderExecOptions.None;
            }
        }

        public static OrderOptions ToExec(this OrderExecOptions options)
        {
            switch (options)
            {
                case OrderExecOptions.ImmediateOrCancel:
                    return OrderOptions.ImmediateOrCancel;

                default:
                    return OrderOptions.None;
            }
        }

        public static string ToFullString(this OrderOptions options)
        {
            var op = new System.Collections.Generic.List<OrderOptions>();

            foreach (var e in Enum.GetValues(typeof(OrderOptions)).Cast<OrderOptions>())
            {
                if (e != OrderOptions.None && options.HasFlag(e))
                    op.Add(e);
            }

            return string.Join(",", op);
        }
    }
}
