using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Calc
{
    public struct StatsChange
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
        string Id { get; }

        /// <summary>
        /// Account type.
        /// </summary>
        Domain.AccountInfo.Types.Type Type { get; }

        /// <summary>
        /// Account orders.
        /// </summary>
        IEnumerable<IOrderCalcInfo> Orders { get; }

        /// <summary>
        /// Fired when single order was added.
        /// </summary>
        event Action<IOrderCalcInfo> OrderAdded;

        /// <summary>
        /// Fired when multiple orders were added.
        /// </summary>
        event Action<IEnumerable<IOrderCalcInfo>> OrdersAdded;

        /// <summary>
        /// Fired when order was removed.
        /// </summary>
        event Action<IOrderCalcInfo> OrderRemoved;

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
        IEnumerable<IPositionInfo> Positions { get; }

        /// <summary>
        /// Fired when position changed.
        /// </summary>
        event Action<IPositionInfo> PositionChanged;
        event Action<IPositionInfo> PositionRemoved;
    }

    public enum PositionChangeTypes
    {
        AddedModified,
        Removed
    }

    /// <summary>
    /// Defines methods and properties for cash account.
    /// </summary>
    public interface ICashAccountInfo2 : IAccountInfo2
    {
        /// <summary>
        /// Cash account assets.
        /// </summary>
        IEnumerable<IAssetInfo> Assets { get; }

        /// <summary>
        /// Fired when underlying assests list was changed.
        /// </summary>
        event Action<IAssetInfo, AssetChangeType> AssetsChanged;
    }
}
