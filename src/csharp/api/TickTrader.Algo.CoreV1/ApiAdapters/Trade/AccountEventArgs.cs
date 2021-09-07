using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreV1
{
    public class OrderActivatedEventArgsImpl : OrderActivatedEventArgs
    {
        public OrderActivatedEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }
    }

    public class OrderOpenedEventArgsImpl : OrderOpenedEventArgs
    {
        public OrderOpenedEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }
    }

    public class OrderModifiedEventArgsImpl : OrderModifiedEventArgs
    {
        public OrderModifiedEventArgsImpl(Order oldOrder, Order newOrder)
        {
            OldOrder = oldOrder;
            NewOrder = newOrder;
        }

        public Order OldOrder { get; }
        public Order NewOrder { get; }
    }

    public class OrderSplittedEventArgsImpl : OrderSplittedEventArgs
    {
        public OrderSplittedEventArgsImpl(Order oldOrder, Order newOrder)
        {
            OldOrder = oldOrder;
            NewOrder = newOrder;
        }

        public Order OldOrder { get; }
        public Order NewOrder { get; }
    }

    public class OrderFilledEventArgsImpl : OrderFilledEventArgs
    {
        public OrderFilledEventArgsImpl(Order oldOrder, Order newOrder)
        {
            OldOrder = oldOrder;
            NewOrder = newOrder;
        }

        public Order OldOrder { get; }
        public Order NewOrder { get; }
    }

    public class OrderClosedEventArgsImpl : OrderClosedEventArgs
    {
        public OrderClosedEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }
    }

    public class OrderCanceledEventArgsImpl : OrderCanceledEventArgs
    {
        public OrderCanceledEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }
    }

    public class OrderExpiredEventArgsImpl : OrderExpiredEventArgs
    {
        public OrderExpiredEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }
    }

    public class PositionModifiedEventArgsImpl : NetPositionModifiedEventArgs
    {
        public PositionModifiedEventArgsImpl(NetPosition oldPos, NetPosition newPos, bool isClosed)
        {
            OldPosition = oldPos;
            NewPosition = newPos;
            IsClosed = isClosed;
        }

        public NetPosition OldPosition { get; }
        public NetPosition NewPosition { get; }
        public bool IsClosed { get; }
    }

    public class PositionSplittedEventArgsImpl : NetPositionSplittedEventArgs
    {
        public PositionSplittedEventArgsImpl(NetPosition oldPos, NetPosition newPos, bool isClosed)
        {
            OldPosition = oldPos;
            NewPosition = newPos;
            IsClosed = isClosed;
        }

        public NetPosition OldPosition { get; }
        public NetPosition NewPosition { get; }
        public bool IsClosed { get; }
    }

    public class AssetUpdateEventArgsImpl : AssetModifiedEventArgs
    {
        public AssetUpdateEventArgsImpl(Asset newAsset)
        {
            NewAsset = newAsset;
        }

        public Asset NewAsset { get; }
    }

    public class BalanceDividendEventArgsImpl : IBalanceDividendEventArgs
    {
        public double Balance { get; set; }

        public string Currency { get; set; }

        public double TransactionAmount { get; set; }

        public BalanceDividendEventArgsImpl(Domain.BalanceOperation report)
        {
            Balance = report.Balance;
            Currency = report.Currency;
            TransactionAmount = report.TransactionAmount;
        }
    }
}
