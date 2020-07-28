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

        decimal IMarginProfitCalc.RemainingAmount => (decimal)Volume;

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
        ISymbolInfo SymbolInfo { get; }
        double CalculateProfitFixedPrice(double openPrice, double volume, double closePrice, Domain.OrderInfo.Types.Side side, out double conversionRate, out CalcErrorCodes error);
        double CalculateSwap(double amount, Domain.OrderInfo.Types.Side side, DateTime now, out CalcErrorCodes error);
        double CalculateProfit(IMarginProfitCalc info);
        double CalculateMargin(IMarginProfitCalc info);
        double CalculateCommission(double amount, double cValue, Domain.CommissonInfo.Types.ValueType vType, out CalcErrorCodes error);
    }

    public enum CalcErrorCodes
    {
        None = 0,
        OffQuote,
        OffCrossQuote,
        NoCrossSymbol,
    }
}
