namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class EMA2 : IMovAvgAlgo
    {
        private int _calcState;
        private double _prev, _current;


        public MovAvgArgs Args { get; }

        public double Average { get; private set; }


        public EMA2(MovAvgArgs args)
        {
            Args = args;
        }


        public void Reset()
        {
            Average = double.NaN;
            _calcState = 0;
            _prev = _current = double.NaN;
        }

        public void Add(double value)
        {
            var k = Args.SmoothFactor;

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

            Average = _current;
        }

        public void UpdateLast(double value)
        {
            if (_calcState == 1)
            {
                _current = value;
            }
            else if (_calcState == 2)
            {
                var k = Args.SmoothFactor;
                _current = k * value + (1 - k) * _prev;
            }

            Average = _current;
        }
    }
}
