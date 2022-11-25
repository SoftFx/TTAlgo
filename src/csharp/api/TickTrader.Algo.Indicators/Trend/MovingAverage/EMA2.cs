namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class EMA2 : IMovAvgAlgo
    {
        private readonly MovAvgArgs _args;

        private int _calcState;
        private double _prev, _current;


        public EMA2(MovAvgArgs args)
        {
            _args = args;
        }

        public void OnInit()
        {
            _calcState = 0;
            _prev = _current = double.NaN;
        }

        public void OnReset()
        {
            _calcState = 0;
            _prev = _current = double.NaN;
        }

        public void OnAdded(double value)
        {
            var k = _args.SmoothFactor;

            if (_calcState == 0)
            {
                _current = value;
                _calcState = 1;
            }
            else
            {
                _prev = _current;
                _current = k * value + (1 - k) * _prev;
                _calcState = 2;
            }
        }

        public void OnLastUpdated(double value)
        {
            if (_calcState == 1)
            {
                _current = value;
            }
            else if (_calcState == 2)
            {
                var k = _args.SmoothFactor;
                _current = k * value + (1 - k) * _prev;
            }
        }

        public double Calculate()
        {
            return _current;
        }
    }
}
