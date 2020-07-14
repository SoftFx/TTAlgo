using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core.Calc
{
    public enum CalcErrorCodes
    {
        None        = 0,
        OffQuote,
        OffCrossQuote,
        NoCrossSymbol,
    }

    public interface IOrderCalcInfo
    {
        //int RangeId { get; }
        //long AccountId { get; }
        string Symbol { get; }
        //string SymbolAlias { get; }
        //int? SymbolPrecision { get; }
        //long OrderId { get; }
        //string ClientOrderId { get; }
        //long? ParentOrderId { get; }
        double? Price { get; }
        double? StopPrice { get; }

        Domain.OrderInfo.Types.Side Side { get; }
        Domain.OrderInfo.Types.Type Type { get; }

        //OrderTypes InitialType { get; }
        ////OrderStatuses Status { get; }
        //double Amount { get; }
        decimal RemainingAmount { get; }
        //double? MaxVisibleAmount { get; }
        //DateTime Created { get; }
        //DateTime? Modified { get; }
        //DateTime? Filled { get; }
        //DateTime? PositionCreated { get; }
        //double? StopLoss { get; }
        //double? TakeProfit { get; }
        //double? Profit { get; }
        //double? Margin { get; }
        //double AggrFillPrice { get; }
        //double AverageFillPrice { get; }
        //double? TransferringCoefficient { get; }
        //string UserComment { get; }
        //string ManagerComment { get; }
        //string UserTag { get; }
        //string ManagerTag { get; }
        //int Magic { get; }
        decimal? Commission { get; }
        //double? AgentCommision { get; }
        decimal? Swap { get; }
        //DateTime? Expired { get; }
        //double? ClosePrice { get; }
        //double? CurrentPrice { get; }
        //double? MarginRateInitial { get; }
        //double? MarginRateCurrent { get; }
        //ActivationTypes Activation { get; }
        //double? OpenConversionRate { get; }
        //double? CloseConversionRate { get; }
        //bool IsReducedOpenCommission { get; }
        //bool IsReducedCloseCommission { get; }
        //int Version { get; }
        //OrderExecutionOptions Options { get; }
        //CustomProperties Properties { get; }
        //double? Taxes { get; }
        //double? ReqOpenPrice { get; }
        //double? ReqOpenAmount { get; }
        //string ClientApp { get; }
        //double? Slippage { get; }
        bool IsHidden { get; }
    }

    public interface IOrderModel2 : IOrderCalcInfo
    {
        OrderCalculator Calculator { get; set; }

        decimal CashMargin { get; set; }
        ISymbolInfo2 SymbolInfo { get; }

        event Action<OrderEssentialsChangeArgs> EssentialsChanged;
        //event Action<OrderPropArgs<decimal>> PriceChanged;
        event Action<OrderPropArgs<decimal>> SwapChanged;
        event Action<OrderPropArgs<decimal>> CommissionChanged;
    }

    public interface IPositionModel2
    {
        string Symbol { get; }
        decimal Commission { get; }
        decimal Swap { get; }
        IPositionSide2 Long { get; } // buy
        IPositionSide2 Short { get; } //sell
        DateTime? Modified { get; }
        OrderCalculator Calculator { get; set; }
    }

    public interface IPositionSide2
    {
        decimal Amount { get; }
        decimal Price { get; }
        decimal Margin { get; set; }
        decimal Profit { get; set; }
    }

    public struct OrderEssentialsChangeArgs
    {
        public OrderEssentialsChangeArgs(IOrderModel2 order, decimal oldRemAmount, double? oldPrice, double? oldStopPrice, Domain.OrderInfo.Types.Type oldType, bool oldIsHidden)
        {
            Order = order;
            OldRemAmount = oldRemAmount;
            OldPrice = oldPrice;
            OldStopPrice = oldStopPrice;
            OldType = oldType;
            OldIsHidden = oldIsHidden;
        }

        public IOrderModel2 Order { get; }
        public decimal OldRemAmount { get; }
        public double? OldPrice { get; }
        public double? OldStopPrice { get; }
        public Domain.OrderInfo.Types.Type OldType { get; }
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
        public StatsChange(double margin, double equity, int errorDelta)
        {
            MarginDelta = margin;
            ProfitDelta = equity;
            ErrorDelta = errorDelta;
        }

        public int ErrorDelta { get; }
        public double MarginDelta { get; }
        public double ProfitDelta { get; }

        public static StatsChange operator +(StatsChange c1, StatsChange c2)
        {
            return new StatsChange(c1.MarginDelta + c2.MarginDelta, c1.ProfitDelta + c2.ProfitDelta, c1.ErrorDelta + c2.ErrorDelta);
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
        Domain.AccountInfo.Types.Type AccountingType { get; }

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
        double Balance { get; }

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
        IEnumerable<IPositionModel2> Positions { get; }

        /// <summary>
        /// Fired when position changed.
        /// </summary>
        event Action<IPositionModel2> PositionChanged;
    }

    //public enum PositionChageTypes
    //{
    //    AddedModified,
    //    Removed
    //}

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
