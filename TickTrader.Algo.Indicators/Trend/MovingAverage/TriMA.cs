namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class TriMA : MABase
    {
        private SMA _innerSma;
        private SMA _outerSma;

        public int SmaPeriod { get; private set; }

        public TriMA(int period) : base(period)
        {
            SmaPeriod = (Period + Period % 2) / 2;
        }

        public override void Init()
        {
            base.Init();
            _innerSma = new SMA(SmaPeriod);
            _outerSma = new SMA(SmaPeriod);
            _innerSma.Init();
            _outerSma.Init();
        }

        public override void Reset()
        {
            base.Reset();
            _innerSma = new SMA(SmaPeriod);
            _outerSma = new SMA(SmaPeriod);
            _innerSma.Init();
            _outerSma.Init();
        }

        protected override void InvokeAdd(double value)
        {
            _innerSma.Add(value);
            if (!double.IsNaN(_innerSma.Result))
            {
                _outerSma.Add(value);
            }
        }

        protected override void InvokeUpdateLast(double value)
        {
            _innerSma.UpdateLast(value);
            if (!double.IsNaN(_innerSma.Result))
            {
                _outerSma.UpdateLast(value);
            }
        }

        protected override void SetCurrentResult()
        {
            Result = _outerSma.Result;
        }
    }
}
