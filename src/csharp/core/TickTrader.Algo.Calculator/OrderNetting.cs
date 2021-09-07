using TickTrader.Algo.Calculator.TradeSpeсificsCalculators;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator
{
    internal sealed class OrderNetting
    {
        private readonly ISymbolCalculator _calculator;
        private readonly OrderInfo.Types.Type _type;
        private readonly OrderInfo.Types.Side _side;
        private readonly bool _isHidden;

        private double _totalAmount = 0.0, _profitAmount = 0.0, _marginAmount = 0.0;
        private double _currentMargin = 0.0, _currentProfit = 0.0;
        private int _errorsCount = 0;

        public bool IsEmpty => _marginAmount <= 0;


        public OrderNetting(ISymbolCalculator calculator, OrderInfo.Types.Type type, OrderInfo.Types.Side side, bool isHidden)
        {
            _calculator = calculator;
            _type = type;
            _side = side;
            _isHidden = isHidden;
        }

        public StatsChange Recalculate()
        {
            var oldMargin = _currentMargin;
            var oldProfit = _currentProfit;
            var oldErros = _errorsCount;

            _errorsCount = 0;

            if (IsEmpty)
            {
                _currentMargin = 0;
                _currentProfit = 0;
            }
            else
            {
                if (_marginAmount > 0.0)
                {
                    var response = _calculator?.Margin?.Calculate(new MarginRequest(_marginAmount, _type, _isHidden));
                    _currentMargin = response?.Value ?? 0.0;

                    if (response == null || response.IsFailed)
                        _errorsCount++;
                }
                else
                    _currentMargin = 0;

                if (_profitAmount > 0.0)
                {
                    var response = _calculator?.Profit?.Calculate(new ProfitRequest(_totalAmount / _profitAmount, _profitAmount, _side));
                    _currentProfit = response?.Value ?? 0.0;

                    if (response == null || response.IsFailed)
                        _errorsCount++;
                }
                else
                    _currentProfit = 0;
            }

            return new StatsChange(_currentMargin - oldMargin, _currentProfit - oldProfit, _errorsCount - oldErros);
        }

        public StatsChange AddOrder(double remAmount, double? price)
        {
            AddOrderWithoutCalculation(remAmount, price);
            return Recalculate();
        }

        public StatsChange RemoveOrder(double remAmount, double? price)
        {
            if (_type.IsPosition())
                RemovePositionWithoutCalculation(remAmount, price ?? 0.0);
            else
                _marginAmount -= remAmount;

            return Recalculate();
        }

        public void AddOrderWithoutCalculation(double remAmount, double? price)
        {
            if (_type.IsPosition())
                AddPositionWithoutCalculation(remAmount, price ?? 0.0);
            else
                _marginAmount += remAmount;
        }

        public void AddPositionWithoutCalculation(double posAmount, double posPrice)
        {
            _marginAmount += posAmount;

            _profitAmount += posAmount;
            _totalAmount += posAmount * posPrice;
        }

        public void RemovePositionWithoutCalculation(double posAmount, double posPrice)
        {
            _marginAmount -= posAmount;

            _profitAmount -= posAmount;
            _totalAmount -= posAmount * posPrice;
        }

        public void UpdateNetPositionWithoutCalculation(double posAmount, double posPrice)
        {
            _marginAmount = posAmount;
            _profitAmount = posAmount;

            _totalAmount = posAmount * posPrice;
        }
    }
}
