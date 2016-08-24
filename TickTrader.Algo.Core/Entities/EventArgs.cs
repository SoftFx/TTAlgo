using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class OrderOpenedEventArgsImpl : OrderOpenedEventArgs
    {
        public OrderOpenedEventArgsImpl(Order order)
        {
            this.Order = order;
        }

        public Order Order { get; private set; }
    }

    public class OrderModifiedEventArgsImpl : OrderModifiedEventArgs
    {
        public OrderModifiedEventArgsImpl(Order newOrder, Order oldOrder)
        {
            this.NewOrder = newOrder;
            this.OldOrder = oldOrder;
        }

        public Order NewOrder { get; private set; }
        public Order OldOrder { get; private set; }
    }

    public class OrderFilledEventArgsImpl : OrderFilledEventArgs
    {
        public OrderFilledEventArgsImpl(Order newOrder, Order oldOrder)
        {
            this.NewOrder = newOrder;
            this.OldOrder = oldOrder;
        }

        public Order NewOrder { get; private set; }
        public Order OldOrder { get; private set; }
    }

    public class OrderClosedEventArgsImpl : OrderClosedEventArgs
    {
        public OrderClosedEventArgsImpl(Order order)
        {
            this.Order = order;
        }

        public Order Order { get; private set; }
    }

    public class OrderCanceledEventArgsImpl : OrderCanceledEventArgs, OrderExpiredEventArgs
    {
        public OrderCanceledEventArgsImpl(Order order)
        {
            this.Order = order;
        }

        public Order Order { get; private set; }
    }
}
