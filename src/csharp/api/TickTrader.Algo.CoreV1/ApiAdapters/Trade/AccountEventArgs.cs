using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.CoreV1
{
    public interface IAccountApiEvent
    {
        void Fire(PluginBuilder builder);
    }

    public class OrderActivatedEventArgsImpl : OrderActivatedEventArgs, IAccountApiEvent
    {
        public OrderActivatedEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.Orders.FireOrderActivated(this);
    }

    public class OrderOpenedEventArgsImpl : OrderOpenedEventArgs, IAccountApiEvent
    {
        public OrderOpenedEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.Orders.FireOrderOpened(this);
    }

    public class OrderModifiedEventArgsImpl : OrderModifiedEventArgs, IAccountApiEvent
    {
        public OrderModifiedEventArgsImpl(Order oldOrder, Order newOrder)
        {
            OldOrder = oldOrder;
            NewOrder = newOrder;
        }

        public Order OldOrder { get; }
        public Order NewOrder { get; }


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.Orders.FireOrderModified(this);
    }

    public class OrderSplittedEventArgsImpl : OrderSplittedEventArgs, IAccountApiEvent
    {
        public OrderSplittedEventArgsImpl(Order oldOrder, Order newOrder)
        {
            OldOrder = oldOrder;
            NewOrder = newOrder;
        }

        public Order OldOrder { get; }
        public Order NewOrder { get; }


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.Orders.FireOrderSplitted(this);
    }

    public class OrderFilledEventArgsImpl : OrderFilledEventArgs, IAccountApiEvent
    {
        public OrderFilledEventArgsImpl(Order oldOrder, Order newOrder)
        {
            OldOrder = oldOrder;
            NewOrder = newOrder;
        }

        public Order OldOrder { get; }
        public Order NewOrder { get; }


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.Orders.FireOrderFilled(this);
    }

    public class OrderClosedEventArgsImpl : OrderClosedEventArgs, IAccountApiEvent
    {
        public OrderClosedEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.Orders.FireOrderClosed(this);
    }

    public class OrderCanceledEventArgsImpl : OrderCanceledEventArgs, IAccountApiEvent
    {
        public OrderCanceledEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.Orders.FireOrderCanceled(this);
    }

    public class OrderExpiredEventArgsImpl : OrderExpiredEventArgs, IAccountApiEvent
    {
        public OrderExpiredEventArgsImpl(Order order)
        {
            Order = order;
        }

        public Order Order { get; }


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.Orders.FireOrderExpired(this);
    }

    public class PositionModifiedEventArgsImpl : NetPositionModifiedEventArgs, IAccountApiEvent
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


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.NetPositions.FirePositionUpdated(this);
    }

    public class PositionSplittedEventArgsImpl : NetPositionSplittedEventArgs, IAccountApiEvent
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


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.NetPositions.FirePositionSplitted(this);
    }

    public class AssetUpdateEventArgsImpl : AssetModifiedEventArgs, IAccountApiEvent
    {
        public AssetUpdateEventArgsImpl(Asset newAsset)
        {
            NewAsset = newAsset;
        }

        public Asset NewAsset { get; }


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.Assets.FireModified(this);
    }

    public class BalanceDividendEventArgsImpl : IBalanceDividendEventArgs, IAccountApiEvent
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


        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.FireBalanceDividendEvent(this);
    }


    public class BalanceUpdatedEventImpl : Singleton<BalanceUpdatedEventImpl>, IAccountApiEvent
    {
        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.FireBalanceUpdateEvent();
    }

    public class AccountResetEventImpl : Singleton<AccountResetEventImpl>, IAccountApiEvent
    {
        void IAccountApiEvent.Fire(PluginBuilder builder) => builder.Account.FireResetEvent();
    }
}
