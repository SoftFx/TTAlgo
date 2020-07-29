using System;

namespace TickTrader.Algo.Domain
{
    public partial class PositionInfo : IPositionInfo
    {
        public bool IsEmpty => Math.Abs(Volume) < 1e-9;

        public IPositionSide Long { get; set; }

        public IPositionSide Short { get; set; }

        public IOrderCalculator Calculator { get; set; }

        decimal IPositionInfo.Amount => Math.Max(Long.Amount, Short.Amount);

        decimal IPositionInfo.Commission => (decimal)Commission;

        decimal IPositionInfo.Swap => (decimal)Swap;

        DateTime? IPositionInfo.Modified => Modified?.ToDateTime();

        double IMarginProfitCalc.Price => (double)(Long.Amount > Short.Amount ? Long.Price : Short.Price);
        decimal IMarginProfitCalc.RemainingAmount => Math.Max(Long.Amount, Short.Amount);
        OrderInfo.Types.Type IMarginProfitCalc.Type => OrderInfo.Types.Type.Position;

        bool IMarginProfitCalc.IsHidden => false;
    }

    public interface IPositionInfo : IMarginProfitCalc
    {
        decimal Amount { get; }
        string Symbol { get; }
        decimal Commission { get; }
        decimal Swap { get; }
        IPositionSide Long { get; } // buy
        IPositionSide Short { get; } //sell
        DateTime? Modified { get; }
        IOrderCalculator Calculator { get; set; }
        bool IsEmpty { get; }
    }

    public interface IPositionSide
    {
        decimal Amount { get; }
        decimal Price { get; }
        decimal Margin { get; set; }
        decimal Profit { get; set; }
    }

    public interface IOrderCalculator
    {
        double CalculateProfit(IMarginProfitCalc info);
        double CalculateMargin(IMarginProfitCalc info);
    }

    public enum CalcErrorCodes
    {
        None = 0,
        OffQuote,
        OffCrossQuote,
        NoCrossSymbol,
    }
}
