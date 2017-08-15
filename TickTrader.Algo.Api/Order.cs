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
        double RequestedVolume { get; }
        double RemainingVolume { get; }
        OrderType Type { get; }
        OrderSide Side { get; }
        double Price { get; }
        double StopLoss { get; }
        double TakeProfit { get; }
        bool IsNull { get; }
        string Comment { get; }
        string Tag { get; }
        string InstanceId { get; }
        DateTime Modified { get; }
        DateTime Created { get; }
        double ExecPrice { get; }
        double ExecVolume { get; }
        double LastFillPrice { get; }
        double LastFillVolume { get; }
        double Margin { get; }
        double Profit { get; }
    }

    public enum OrderType { Market, Limit, Stop, StopLimit, Position }
    public enum OrderSide { Buy, Sell }

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
        event Action<Order> Added;
        event Action<Order> Removed;
        event Action<Order> Replaced;
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
