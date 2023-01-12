namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class TriMA2 : IMovAvgAlgo
    {
        private readonly SMA2 _innerSma, _outerSma;


        public MovAvgArgs Args { get; }

        public double Average { get; private set; }


        public TriMA2(MovAvgArgs args)
        {
            Args = args;

            var period = args.Period;

            int innerSmaPeriod, outerSmaPeriod;
            if (period % 2 == 0)
            {
                innerSmaPeriod = period / 2 + 1;
                outerSmaPeriod = period / 2;
            }
            else
            {
                innerSmaPeriod = outerSmaPeriod = (period + 1) / 2;
            }


            _innerSma = new SMA2(new MovAvgArgs(Api.Indicators.MovingAverageMethod.Simple, innerSmaPeriod, double.NaN));
            _outerSma = new SMA2(new MovAvgArgs(Api.Indicators.MovingAverageMethod.Simple, outerSmaPeriod, double.NaN));
        }


        public void Reset()
        {
            Average = double.NaN;
            _innerSma.Reset();
            _outerSma.Reset();
        }

        public void Add(double value)
        {
            _innerSma.Add(value);
            if (!double.IsNaN(_innerSma.Average))
            {
                _outerSma.Add(_innerSma.Average);
            }

            Average = _outerSma.Average;
        }

        public void UpdateLast(double value)
        {
            _innerSma.UpdateLast(value);
            if (!double.IsNaN(_innerSma.Average))
            {
                _outerSma.UpdateLast(_innerSma.Average);
            }

            Average = _outerSma.Average;
        }
    }
}
