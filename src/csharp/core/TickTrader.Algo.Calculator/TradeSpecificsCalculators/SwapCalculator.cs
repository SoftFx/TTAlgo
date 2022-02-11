using System;
using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.TradeSpecificsCalculators
{
    interface ISwapCalculationInfo
    {
        bool Enabled { get; }

        SwapInfo.Types.Type Type { get; }

        int TripleSwapDay { get; }

        double? SwapSizeLong { get; }

        double? SwapSizeShort { get; }

        int SymbolDigits { get; }
    }


    public sealed class SwapCalculator : ISwapCalculator
    {
        private readonly ISwapCalculationInfo _info;
        private readonly IConversionFormula _marginRate, _positiveRate, _negativeRate;
        private double _swapModifierLong, _swapModifierShort;


        internal SwapCalculator(ISwapCalculationInfo info, IConversionFormula marginFormula, IConversionFormula positiveFormula, IConversionFormula negativeFormula)
        {
            _info = info;
            _marginRate = marginFormula;
            _positiveRate = positiveFormula;
            _negativeRate = negativeFormula;

            _swapModifierLong = CalculateSwapModifier(OrderInfo.Types.Side.Buy);
            _swapModifierShort = CalculateSwapModifier(OrderInfo.Types.Side.Sell);
        }


        public ICalculateResponse<double> Calculate(ISwapCalculateRequest request, DateTime now)
        {
            if (!_info.Enabled)
                return ResponseFactory.Build(0, CalculationError.None);

            double swapModifier = 0.0;
            var side = request.Side;
            if (side.IsBuy())
                swapModifier = _swapModifierLong;
            else if (side.IsSell())
                swapModifier = _swapModifierShort;

            if (_info.TripleSwapDay > 0)
            {
                var nowDayOfWeek = now.DayOfWeek;
                if (nowDayOfWeek == DayOfWeek.Saturday || nowDayOfWeek == DayOfWeek.Sunday)
                    return ResponseFactory.Build(0, CalculationError.None);
                else if (_info.TripleSwapDay == nowDayOfWeek - DayOfWeek.Monday)
                    swapModifier *= 3;
            }

            IConversionFormula convertionRate = _marginRate;
            double swapAmount = swapModifier * request.Volume;
            if (_info.Type == SwapInfo.Types.Type.Points)
            {
                convertionRate = swapAmount < 0 ? _negativeRate : _positiveRate;
            }

            return ResponseFactory.Build(swapAmount * convertionRate.Value, convertionRate.Error);
        }


        private double CalculateSwapModifier(OrderInfo.Types.Side side)
        {
            if (_info.Enabled)
            {
                double swapSize = 0.0;
                if (side.IsBuy())
                    swapSize = _info.SwapSizeLong ?? 0;
                else if (side.IsSell())
                    swapSize = _info.SwapSizeShort ?? 0;

                if (_info.Type == SwapInfo.Types.Type.Points)
                {
                    return swapSize * Math.Pow(10, -_info.SymbolDigits);
                }
                else if (_info.Type == SwapInfo.Types.Type.PercentPerYear)
                {
                    const double power = 1.0 / 365.0;

                    double factor = Math.Sign(swapSize) * (Math.Pow(1 + Math.Abs(swapSize), power) - 1);

                    //if (double.IsInfinity(factor) || double.IsNaN(factor))
                    //    throw new MarketConfigurationException($"Can not calculate swap: side={side} symbol={SymbolInfo.Symbol} swaptype={SymbolInfo.SwapType} sizelong={SymbolInfo.SwapSizeLong} sizeshort={SymbolInfo.SwapSizeShort}");

                    return factor;
                }
            }

            return 0;
        }
    }
}
