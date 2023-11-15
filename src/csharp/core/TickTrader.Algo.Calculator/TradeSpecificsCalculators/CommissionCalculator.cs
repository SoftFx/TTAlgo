using System;
using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Core.Lib;
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

        private readonly IConversionFormula _marginRate, _negativeRate, _minCommissionRate;
        private readonly double _takerModifier, _makerModifier;
        private readonly bool _isEnabled;

        internal CommissionCalculator(ICommissionCalculationInfo info, IConversionFormula marginRate, IConversionFormula negativeRate, IConversionFormula minCommissionRate)
        {
            _info = info;
            _marginRate = marginRate;
            _negativeRate = negativeRate;
            _minCommissionRate = minCommissionRate;

            _isEnabled = !(_info.TakerFee.E(0) && _info.MakerFee.E(0) && _info.MinCommission.E(0));

            _takerModifier = CalculateCommissionModifier(_info.TakerFee);
            _makerModifier = CalculateCommissionModifier(_info.MakerFee);
        }

        public ICalculateResponse<double> Calculate(ICommissionCalculateRequest request)
        {
            if (!_isEnabled)
                return ResponseFactory.Build(0, CalculationError.None);

            var minCommissionAmount = 0.0;
            //if (_minCommissionRate.Error == CalculationError.None)
            //    minCommissionAmount = _info.MinCommission * _minCommissionRate.Value;

            var commissionModifier = request.IsReducedCommission ? _makerModifier : _takerModifier;
            double commissionAmount = commissionModifier * request.Volume;
            CalculationError calcError = CalculationError.None;
            switch (_info.Type)
            {
                case CommissonInfo.Types.ValueType.Points:
                    commissionAmount *= _negativeRate.Value;
                    calcError = _negativeRate.Error;
                    break;
                case CommissonInfo.Types.ValueType.Percentage:
                    commissionAmount *= _marginRate.Value;
                    calcError = _marginRate.Error;
                    break;
            }

            commissionAmount = -Math.Max(commissionAmount, minCommissionAmount);

            return ResponseFactory.Build(commissionAmount, calcError);
        }


        private double CalculateCommissionModifier(double commissionValue)
        {
            switch (_info.Type)
            {
                case CommissonInfo.Types.ValueType.Points:
                    return commissionValue * Math.Pow(10, -_info.SymbolDigits);
                case CommissonInfo.Types.ValueType.Percentage:
                    return commissionValue / 100;
                case CommissonInfo.Types.ValueType.Money:
                    return commissionValue / _info.LotSize;
            }

            return 0;
        }
    }
}
