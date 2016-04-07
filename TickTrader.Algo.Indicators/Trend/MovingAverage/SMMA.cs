using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class SMMA : MABase
    {
        private double _sum;
        private double _prev;
        private double _prevsum;

        public SMMA(int period, int shift, AppliedPrice.Target targetPrice) : base(period, shift, targetPrice)
        {
        }

        public override void Init()
        {
            base.Init();
            _sum = 0;
            _prev = double.NaN;
            _prevsum = double.NaN;
        }

        public override void Reset()
        {
            base.Reset();
            _sum = 0;
            _prev = double.NaN;
            _prev = double.NaN;
        }

        public override double Calculate(Bar bar)
        {
            var appliedPrice = AppliedPrice.Calculate(bar, TargetPrice);
            if (Accumulated < Period)
            {
                Accumulated++;
                _sum += appliedPrice;
                if (Accumulated < Period)
                {
                    return double.NaN;
                }
            }
            double res;
            if (double.IsNaN(_prev))
            {
                res = _sum/Period;
            }
            else if (double.IsNaN(_prevsum))
            {
                _prevsum = _sum - _prev + appliedPrice;
                res = _prevsum/Period;
            }
            else
            {
                _prevsum = _prevsum - _prev + appliedPrice;
                res = _prevsum/Period;
            }
            _prev = res;

            return res;
        }
    }
}
