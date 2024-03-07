using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.TradeSpecificsCalculators
{
    internal interface IMarginCalculationInfo
    {
        int Leverage { get; }

        double Factor { get; }

        double? StopOrderReduction { get; }

        double? HiddenLimitOrderReduction { get; }
    }

    // https://intranet.fxopen.org/wiki/display/TIC/Margin
    public sealed class MarginCalculator : IMarginCalculator
    {
        private readonly IConversionFormula _convertionRate;
        private readonly IMarginCalculationInfo _info;

        private readonly double _marginFactor;
        private readonly double _stopFactor;
        private readonly double _hiddenFactor;

        internal MarginCalculator(IMarginCalculationInfo info, IConversionFormula formula)
        {
            _info = info;
            _convertionRate = formula;

            _marginFactor = _info.Factor;
            _stopFactor = _marginFactor * _info.StopOrderReduction ?? 1;
            _hiddenFactor = _marginFactor * _info.HiddenLimitOrderReduction ?? 1;
        }

        public ICalculateResponse<double> Calculate(IMarginCalculateRequest request)
        {
            var margin = request.Volume * _convertionRate.Value * MarginFactor(request) / _info.Leverage;

            return ResponseFactory.Build(margin, _convertionRate.Error);
        }

        public double? GetConversionRate()
        {
            var convertionRate = _convertionRate;
            return convertionRate.Error == CalculationError.None ? convertionRate.Value : default(double?);
        }


        private double MarginFactor(IMarginCalculateRequest request)
        {
            if (request.Type.IsStop())
                return _stopFactor;

            if (request.IsHiddenLimit)
                return _hiddenFactor;

            return _marginFactor;
        }
    }
}
