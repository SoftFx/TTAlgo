namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class EMA : MABase
    {
        private double _prev;

        public double SmoothFactor { get; private set; }

        public EMA(int period) : base(period)
        {
            SmoothFactor = 2.0/(period + 1.0);
        }

        public override void Init()
        {
            base.Init();
            _prev = double.NaN;
        }

        public override void Reset()
        {
            base.Reset();
            _prev = double.NaN;
        }

        protected override void InvokeAdd(double value)
        {
            _prev = double.IsNaN(_prev) ? value : SmoothFactor*value + (1 - SmoothFactor)*_prev;
        }

        protected override void InvokeUpdateLast(double value)
        {
            _prev = (Accumulated == 1) ? value : _prev + SmoothFactor*value - SmoothFactor*LastAdded;
        }

        protected override void SetCurrentResult()
        {
            Average = _prev;
        }
    }
}
