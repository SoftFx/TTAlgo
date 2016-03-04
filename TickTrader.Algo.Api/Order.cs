using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Order
    {
        long Id { get; }
        string Symbol { get; }
        decimal TotalAmount { get; }
        decimal RemainingAmount { get; }
        OrderTypes Type { get; }
    }

    public enum OrderTypes { Market, Limit, Stop, Position }

    public interface OrderList : IReadOnlyList<Position>
    {
        event Action<OrderOpenedEventArgs> Opened;
        event Action<OrderClosedEventArgs> Closed;
        event Action<OrderModifiedEventArgs> Modified;
    }

    public interface OrderOpenedEventArgs
    {
        Order Order { get; }
    }

    public interface OrderClosedEventArgs
    {
        Order Order { get; }
    }

    public interface OrderModifiedEventArgs
    {
        Order OldOrder { get; }
        Order NewOrder { get; }
    }
}
