using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core.Calc
{
    public interface IOrderModel2 : IOrder
    {
        bool IsHidden { get; }
        OrderCalculator Calculator { get; set; }

        event Action<OrderEssentialsChangeArgs> EssentialsChanged;
        //event Action<OrderPropArgs<decimal>> PriceChanged;
        event Action<OrderPropArgs<decimal>> SwapChanged;
        event Action<OrderPropArgs<decimal>> CommissionChanged;
    }

    public struct OrderEssentialsChangeArgs
    {
        public OrderEssentialsChangeArgs(IOrderModel2 order, decimal oldRemAmount, decimal? oldPrice, OrderTypes oldType, bool oldIsHidden)
        {
            Order = order;
            OldRemAmount = oldRemAmount;
            OldPrice = oldPrice;
            OldType = oldType;
            OldIsHidden = oldIsHidden;
        }

        public IOrderModel2 Order { get; }
        public decimal OldRemAmount { get; }
        public decimal? OldPrice { get; }
        public OrderTypes OldType { get; }
        public bool OldIsHidden { get; }
    }

    public struct OrderPropArgs<T>
    {
        public OrderPropArgs(IOrderModel2 order, T oldVal, T newVal)
        {
            Order = order;
            OldVal = oldVal;
            NewVal = newVal;
        }

        public IOrderModel2 Order { get; }
        public T OldVal { get; }
        public T NewVal { get; }
    }

    internal struct StatsChange
    {
        public StatsChange(decimal margin, decimal equity)
        {
            MarginDelta = margin;
            ProfitDelta = equity;
        }

        public decimal MarginDelta { get; }
        public decimal ProfitDelta { get; }

        public static StatsChange operator +(StatsChange c1, StatsChange c2)
        {
            return new StatsChange(c1.MarginDelta + c2.MarginDelta, c1.ProfitDelta + c2.ProfitDelta);
        }
    }

    public interface IAccountInfo2
    {
        /// <summary>
        /// Account Id.
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Account type.
        /// </summary>
        AccountingTypes AccountingType { get; }

        /// <summary>
        /// Account orders.
        /// </summary>
        IEnumerable<IOrderModel2> Orders { get; }

        /// <summary>
        /// Fired when single order was added.
        /// </summary>
        event Action<IOrderModel2> OrderAdded;

        /// <summary>
        /// Fired when multiple orders were added.
        /// </summary>
        event Action<IEnumerable<IOrderModel2>> OrdersAdded;

        /// <summary>
        /// Fired when order was removed.
        /// </summary>
        event Action<IOrderModel2> OrderRemoved;

        /// <summary>
        /// Fired when order was replaced.
        /// </summary>
        //event Action<IOrderModel2> OrderReplaced;
    }

    /// <summary>
    /// Defines methods and properties for marginal account.
    /// </summary>
    public interface IMarginAccountInfo2 : IAccountInfo2
    {
        /// <summary>
        /// Account balance.
        /// </summary>
        decimal Balance { get; }

        /// <summary>
        /// Account leverage.
        /// </summary>
        int Leverage { get; }

        /// <summary>
        /// Account currency.
        /// </summary>
        string BalanceCurrency { get; }

        /// <summary>
        /// Account positions.
        /// </summary>
        //IEnumerable<IPositionModel> Positions { get; }

        /// <summary>
        /// Fired when position changed.
        /// </summary>
        event Action<IPositionModel, PositionChageTypes> PositionChanged;
    }

    /// <summary>
    /// Defines methods and properties for cash account.
    /// </summary>
    public interface ICashAccountInfo2: IAccountInfo2
    {
        /// <summary>
        /// Cash account assets.
        /// </summary>
        IEnumerable<IAssetModel> Assets { get; }

        /// <summary>
        /// Fired when underlying assests list was changed.
        /// </summary>
        event Action<IAssetModel, AssetChangeTypes> AssetsChanged;
    }
}
