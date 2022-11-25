namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class TriMA2 : IMovAvgAlgo
    {
        private readonly MovAvg _innerSma, _outerSma;


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

            _innerSma = MovAvg.Create(innerSmaPeriod, Api.Indicators.MovingAverageMethod.Simple);
            _outerSma = MovAvg.Create(outerSmaPeriod, Api.Indicators.MovingAverageMethod.Simple);
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
