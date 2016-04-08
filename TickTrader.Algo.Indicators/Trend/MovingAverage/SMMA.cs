namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class SMMA : MABase
    {
        private double _sum;
        private double _prev;
        private double _prevsum;

        public SMMA(int period) : base(period)
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

        protected override void InvokeAdd(double value)
        {
            if (Accumulated <= Period)
            {
                _sum += value;
            }
            if (Accumulated >= Period)
            {
                if (double.IsNaN(_prev))
                {
                    _prev = _sum/Period;
                }
                else if (double.IsNaN(_prevsum))
                {
                    _prevsum = _sum - _prev + value;
                    _prev = _prevsum/Period;
                }
                else
                {
                    _prevsum = _prevsum - _prev + value;
                    _prev = _prevsum/Period;
                }
            }
        }

        protected override void InvokeUpdateLast(double value)
        {
            if (Accumulated <= Period)
            {
                _sum += value - LastAdded;
            }
            if (Accumulated == Period)
            {
                _prev = _sum/Period;
            }
            else
            {
                _prevsum += value - LastAdded;
                _prev = _prevsum/Period;
            }
        }

        protected override void SetCurrentResult()
        {
            Average = Accumulated < Period ? double.NaN : _prev;
        }
    }
}
