namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class TriMA : MABase
    {
        private SMA _innerSma;
        private SMA _outerSma;

        public int InnerSmaPeriod { get; private set; }
        public int OuterSmaPeriod { get; private set; }

        public TriMA(int period) : base(period)
        {
            InnerSmaPeriod = (Period + Period%2)/2 + 1 - Period%2;
            OuterSmaPeriod = (Period + Period%2)/2;
        }

        public override void Init()
        {
            base.Init();
            _innerSma = new SMA(InnerSmaPeriod);
            _outerSma = new SMA(OuterSmaPeriod);
            _innerSma.Init();
            _outerSma.Init();
        }

        public override void Reset()
        {
            base.Reset();
            _innerSma = new SMA(InnerSmaPeriod);
            _outerSma = new SMA(OuterSmaPeriod);
            _innerSma.Init();
            _outerSma.Init();
        }

        protected override void InvokeAdd(double value)
        {
            _innerSma.Add(value);
            if (!double.IsNaN(_innerSma.Average))
            {
                _outerSma.Add(_innerSma.Average);
            }
        }

        protected override void InvokeUpdateLast(double value)
        {
            _innerSma.UpdateLast(value);
            if (!double.IsNaN(_innerSma.Average))
            {
                _outerSma.UpdateLast(_innerSma.Average);
            }
        }

        protected override void SetCurrentResult()
        {
            Average = _outerSma.Average;
        }
    }
}
