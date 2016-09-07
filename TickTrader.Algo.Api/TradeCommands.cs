using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface TradeCommands
    {
        Task<OrderCmdResult> OpenOrder(string symbol, OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment);
        Task<OrderCmdResult> CancelOrder(string orderId);
        Task<OrderCmdResult> ModifyOrder(string orderId, double price, double? tp, double? sl, string comment);
        Task<OrderCmdResult> CloseOrder(string orderId, double? volume);
    }

    public interface OrderCmdResult
    {
        OrderCmdResultCodes ResultCode { get; }
        bool IsCompleted { get; }
        bool IsFaulted { get; }
        Order ResultingOrder { get; }
    }

    public enum OrderCmdResultCodes
    {
        Ok,
        DealerReject,
        ConnectionError,
        Unsupported,
        SymbolNotFound,
        OrderNotFound
    }
}
