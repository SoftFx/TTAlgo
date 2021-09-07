using System;

namespace TickTrader.Algo.Calculator.TradeSpeсificsCalculators
{
    interface ICalculationSwapInfo
    {
        bool Enabled { get; }

        Domain.SwapInfo.Types.Type Type { get; }

        int TripleSwapDay { get; }
    }


    //internal sealed class SwapCalculator
    //{
    //    private readonly ICalculationSwapInfo _swapInfo;


    //    internal SwapCalculator(ICalculationSwapInfo info)
    //    {
    //        _swapInfo = info;
    //    }

    //    public double CalculateSwap(double amount, Domain.OrderInfo.Types.Side side, DateTime now, out CalcErrorCodes error)
    //    {
    //        error = CalcErrorCodes.None;

    //        double swapAmount = GetSwapModifier(side) * amount;
    //        double swap = 0;

    //        if (SymbolInfo.Swap.Type == Domain.SwapInfo.Types.Type.Points)
    //            swap = ConvertProfitToAccountCurrency(swapAmount, out error);
    //        else if (SymbolInfo.Swap.Type == Domain.SwapInfo.Types.Type.PercentPerYear)
    //            swap = ConvertMarginToAccountCurrency(swapAmount, out error);

    //        if (SymbolInfo.Swap.TripleSwapDay > 0)
    //        {
    //            //var now = DateTime.UtcNow;
    //            DayOfWeek swapDayOfWeek = now.DayOfWeek == DayOfWeek.Sunday ? DayOfWeek.Saturday : (int)now.DayOfWeek - DayOfWeek.Monday;
    //            if (SymbolInfo.Swap.TripleSwapDay == (int)swapDayOfWeek)
    //                swap *= 3;
    //            else if (swapDayOfWeek == DayOfWeek.Saturday || swapDayOfWeek == DayOfWeek.Sunday)
    //                swap = 0;
    //        }

    //        return swap;
    //    }

    //    private double GetSwapModifier(Domain.OrderInfo.Types.Side side)
    //    {
    //        if (SymbolInfo.Swap.Enabled)
    //        {
    //            if (SymbolInfo.Swap.Type == Domain.SwapInfo.Types.Type.Points)
    //            {
    //                if (side == Domain.OrderInfo.Types.Side.Buy)
    //                    return SymbolInfo.Swap.SizeLong / Math.Pow(10, SymbolInfo.Digits) ?? 0;
    //                if (side == Domain.OrderInfo.Types.Side.Sell)
    //                    return SymbolInfo.Swap.SizeShort / Math.Pow(10, SymbolInfo.Digits) ?? 0;
    //            }
    //            else if (SymbolInfo.Swap.Type == Domain.SwapInfo.Types.Type.PercentPerYear)
    //            {
    //                const double power = 1.0 / 365.0;
    //                double factor = 0.0;
    //                if (side == Domain.OrderInfo.Types.Side.Buy)
    //                    factor = Math.Sign(SymbolInfo.Swap.SizeLong ?? 0) * (Math.Pow(1 + Math.Abs(SymbolInfo.Swap.SizeLong ?? 0), power) - 1);
    //                if (side == Domain.OrderInfo.Types.Side.Sell)
    //                    factor = Math.Sign(SymbolInfo.Swap.SizeShort ?? 0) * (Math.Pow(1 + Math.Abs(SymbolInfo.Swap.SizeShort ?? 0), power) - 1);

    //                //if (double.IsInfinity(factor) || double.IsNaN(factor))
    //                //    throw new MarketConfigurationException($"Can not calculate swap: side={side} symbol={SymbolInfo.Symbol} swaptype={SymbolInfo.SwapType} sizelong={SymbolInfo.SwapSizeLong} sizeshort={SymbolInfo.SwapSizeShort}");

    //                return factor;
    //            }
    //        }

    //        return 0;
    //    }

    //    public double ConvertProfitToAccountCurrency(double profit, out CalcErrorCodes error)
    //    {
    //        double conversionRate;

    //        if (profit >= 0)
    //        {
    //            error = PositiveProfitConversionRate.ErrorCode;
    //            conversionRate = PositiveProfitConversionRate.Value;
    //        }
    //        else
    //        {
    //            error = NegativeProfitConversionRate.ErrorCode;
    //            conversionRate = NegativeProfitConversionRate.Value;
    //        }

    //        if (error == CalcErrorCodes.None)
    //            return profit * conversionRate;

    //        return 0;
    //    }
    //}
}
