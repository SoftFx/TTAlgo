namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class TriMA2 : IMovAvgAlgo
    {
        private readonly MovAvgArgs _args;
        private readonly MovAvg _innerSma, _outerSma;


        public TriMA2(MovAvgArgs args)
        {
            _args = args;

            var period = _args.Period;

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

        public void OnInit()
        {
            _innerSma.Init();
            _outerSma.Init();
        }

        public void OnReset()
        {
            _innerSma.Reset();
            _outerSma.Reset();
        }

        public void OnAdded(double value)
        {
            _innerSma.Add(value);
            if (!double.IsNaN(_innerSma.Average))
            {
                _outerSma.Add(_innerSma.Average);
            }
        }

        public void OnLastUpdated(double value)
        {
            _innerSma.UpdateLast(value);
            if (!double.IsNaN(_innerSma.Average))
            {
                _outerSma.UpdateLast(_innerSma.Average);
            }
        }

        public double Calculate()
        {
            return _outerSma.Average;
        }
    }
}
