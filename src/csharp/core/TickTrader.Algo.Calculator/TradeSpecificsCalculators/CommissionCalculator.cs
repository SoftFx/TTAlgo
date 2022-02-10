using System;
using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.TradeSpecificsCalculators
{
    internal interface ICommissionCalculationInfo
    {
        CommissonInfo.Types.ValueType Type { get; }

        double LotSize { get; }

        double SymbolDigits { get; }

        double TakerFee { get; }

        double MakerFee { get; }

        double MinCommission { get; }
    }

    internal sealed class CommissionCalculator: ICommissionCalculator
    {
        private readonly ICommissionCalculationInfo _info;

        private readonly IConversionFormula _marginRate;
        private readonly IProfitCalculator _profitCalculator;

        internal CommissionCalculator(ICommissionCalculationInfo info, IConversionFormula marginRate, IProfitCalculator profitCalculator)
        {
            _info = info;
            _marginRate = marginRate;
            _profitCalculator = profitCalculator;
        }

        public ICalculateResponse<double> Calculate(ICommissionCalculateRequest request)
        {
            return ResponseFactory.Build(0, CalculationError.None);
        }

        public double CalculateCommission(double amount, double cValue, out CalculationError error)
        {
            double commission = -amount * cValue;
            error = CalculationError.None;

            if (cValue == 0)
                return 0;

            switch (_info.Type)
            {
                case CommissonInfo.Types.ValueType.Money:
                    commission /= _info.LotSize;
                    break;

                case CommissonInfo.Types.ValueType.Percentage:
                    error = _marginRate.Error;
                    commission *= _marginRate.Value / 100;
                    break;

                case CommissonInfo.Types.ValueType.Points:
                    //_profit convertion rate error???
                    //commission *= _info.Point * _marginRate.Value;
                    //commission = _profitCalculator.ConvertionRate(commission).Value;
                    break;

                default:
                    throw new ArgumentException($"Invalid commission value type = {_info.Type}");
            }

            return error == CalculationError.None ? commission : 0.0;
        }
    }
}
