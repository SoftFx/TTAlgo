using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Core.Lib.Math;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.TradeSpecificsCalculators
{
    internal interface IProfitCalculationInfo
    {
        ISideNode Bid { get; }

        ISideNode Ask { get; }
    }

    // https://intranet.fxopen.org/wiki/display/TIC/Profit
    public sealed class ProfitCalculator : IProfitCalculator
    {
        private readonly IConversionFormula _positiveRate;
        private readonly IConversionFormula _negativeRate;
        private readonly IProfitCalculationInfo _info;

        internal ProfitCalculator(IProfitCalculationInfo info, IConversionFormula positiveFormula, IConversionFormula negativeFormula)
        {
            _info = info;
            _positiveRate = positiveFormula;
            _negativeRate = negativeFormula;
        }

        public ICalculateResponse<double> Calculate(IProfitCalculateRequest request)
        {
            var profit = PriceDelta(request) * request.Volume;
            var convertionRate = ConvertionRate(profit);

            return ResponseFactory.Build(profit * convertionRate.Value, convertionRate.Error);
        }

        private double PriceDelta(IProfitCalculateRequest request)
        {
            if (request.Side.IsBuy() && _info.Bid.HasValue)
                return _info.Bid.Value - request.Price;

            if (request.Side.IsSell() && _info.Ask.HasValue)
                return request.Price - _info.Ask.Value;

            return 0.0;
        }

        private IConversionFormula ConvertionRate(double rate) => rate.Gte(0.0) ? _positiveRate : _negativeRate;
    }
}
