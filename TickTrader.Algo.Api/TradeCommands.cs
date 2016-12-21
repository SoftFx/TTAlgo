using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface TradeCommands
    {
        Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volume, double price, double? sl, double? tp, string comment, OrderExecOptions options, string tag);
        Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId);
        Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price, double? sl, double? tp, string comment);
        Task<OrderCmdResult> CloseOrder(bool isAysnc, string orderId, double? volume);
    }

    public interface OrderCmdResult
    {
        OrderCmdResultCodes ResultCode { get; }
        bool IsCompleted { get; }
        bool IsFaulted { get; }
        Order ResultingOrder { get; }
    }

    [Flags]
    public enum OrderExecOptions
    {
        None                = 0,
        ImmediateOrCancel   = 1,
    }

    public enum OrderCmdResultCodes
    {
        Ok                  = 0,
        UnknownError        = 1,
        InternalError       = 5,
        ConnectionError     = 6,
        Timeout             = 7,
        DealerReject        = 100,
        Unsupported         = 101,
        SymbolNotFound      = 102,
        OrderNotFound       = 103,
        IncorrectVolume     = 104,
        OffQuotes           = 105,
        IncorrectOrderId    = 106,
        IncorrectPrice      = 107,
        IncorrectTp         = 108,
        IncorrectSl         = 109,
        NotEnoughMoney      = 110,
        TradeNotAllowed     = 111
    }
}
