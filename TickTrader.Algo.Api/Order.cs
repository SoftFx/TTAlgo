using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Order
    {
        string Id { get; }
        string Symbol { get; }
        double RequestedAmount { get; }
        double RemainingAmount { get; }
        OrderTypes Type { get; }
        OrderSides Side { get; }
        double Price { get; }
    }

    public enum OrderTypes { Market, Limit, Stop, Position }
    public enum OrderSides { Buy, Sell }

    public interface OrderList : IEnumerable<Order>
    {
        int Count { get; }

        Order this[string id] { get; }

        event Action<OrderOpenedEventArgs> Opened;
        event Action<OrderCanceledEventArgs> Canceled;
        event Action<OrderClosedEventArgs> Closed;
        event Action<OrderModifiedEventArgs> Modified;
        event Action<OrderFilledEventArgs> Filled;
        event Action<OrderExpiredEventArgs> Expired;
    }

    public interface OrderOpenedEventArgs
    {
        Order Order { get; }
    }

    public interface OrderClosedEventArgs
    {
        Order Order { get; }
    }

    public interface OrderExpiredEventArgs
    {
        Order Order { get; }
    }

    public interface OrderCanceledEventArgs
    {
        Order Order { get; }
    }

    public interface OrderModifiedEventArgs
    {
        Order OldOrder { get; }
        Order NewOrder { get; }
    }

    public interface OrderFilledEventArgs
    {
        Order OldOrder { get; }
        Order NewOrder { get; }
    }
}
