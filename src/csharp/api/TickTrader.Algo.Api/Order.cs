using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Api
{
    public interface Order
    {
        string Id { get; }
        string Symbol { get; }
        double RequestedVolume { get; }
        double RemainingVolume { get; }
        double MaxVisibleVolume { get; }
        OrderType Type { get; }
        OrderSide Side { get; }
        double Price { get; }
        double StopPrice { get; }
        double StopLoss { get; }
        double TakeProfit { get; }
        double Slippage { get; }
        bool IsNull { get; }
        string Comment { get; }
        string Tag { get; }
        string InstanceId { get; }
        DateTime Modified { get; }
        DateTime Created { get; }
        DateTime Expiration { get; }
        double ExecPrice { get; }
        double ExecVolume { get; }
        double LastFillPrice { get; }
        double LastFillVolume { get; }
        double Margin { get; }
        double Profit { get; }
        OrderOptions Options { get; }
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
        event Action<OrderActivatedEventArgs> Activated;
        event Action<OrderSplittedEventArgs> Splitted;
        event Action<Order> Added;
        event Action<Order> Removed;
        event Action<Order> Replaced;
    }

    public interface SingleOrderEventArgs
    {
        Order Order { get; }
    }

    public interface DoubleOrderEventArgs
    {
        Order OldOrder { get; }
        Order NewOrder { get; }
    }

    public interface OrderActivatedEventArgs : SingleOrderEventArgs { }

    public interface OrderOpenedEventArgs : SingleOrderEventArgs { }

    public interface OrderClosedEventArgs : SingleOrderEventArgs { }

    public interface OrderExpiredEventArgs : SingleOrderEventArgs { }

    public interface OrderCanceledEventArgs : SingleOrderEventArgs { }

    public interface OrderModifiedEventArgs : DoubleOrderEventArgs { }

    public interface OrderFilledEventArgs : DoubleOrderEventArgs { }

    public interface OrderSplittedEventArgs : DoubleOrderEventArgs { }
}
