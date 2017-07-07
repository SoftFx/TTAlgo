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
        public OrderModifiedEventArgsImpl(Order oldOrder, Order newOrder)
        {
            this.OldOrder = oldOrder;
            this.NewOrder = newOrder;
        }

        public Order OldOrder { get; private set; }
        public Order NewOrder { get; private set; }
    }

    public class OrderFilledEventArgsImpl : OrderFilledEventArgs
    {
        public OrderFilledEventArgsImpl(Order oldOrder, Order newOrder)
        {
            this.OldOrder = oldOrder;
            this.NewOrder = newOrder;
        }

        public Order OldOrder { get; private set; }
        public Order NewOrder { get; private set; }
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

    public class PositionModifiedEventArgsImpl : NetPositionModifiedEventArgs
    {
        public PositionModifiedEventArgsImpl(NetPosition oldPos, NetPosition newPos, bool isClosed)
        {
            OldPosition = oldPos;
            NewPosition = newPos;
            IsClosed = isClosed;
        }

        public NetPosition OldPosition { get; private set; }
        public NetPosition NewPosition { get; private set; }
        public bool IsClosed { get; private set; }
    }

    public class AssetUpdateEventArgsImpl : AssetModifiedEventArgs
    {
        public AssetUpdateEventArgsImpl(Asset newAsset)
        {
            this.NewAsset = newAsset;
        }

        public Asset NewAsset { get; private set; }
    }
}
