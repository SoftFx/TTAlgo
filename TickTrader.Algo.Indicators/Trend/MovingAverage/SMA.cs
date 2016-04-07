using System.Collections.Generic;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class SMA : MABase
    {
        private double _sum;
        private LinkedList<double> _valueCache;

        public SMA(int period) : base(period)
        {
        }

        public override void Init()
        {
            base.Init();
            _sum = 0;
            _valueCache = new LinkedList<double>();
        }

        public override void Reset()
        {
            base.Reset();
            _sum = 0;
            _valueCache.Clear();
        }

        protected override void InvokeAdd(double value)
        {
            if (Accumulated > Period)
            {
                _sum -= _valueCache.First.Value;
                _valueCache.RemoveFirst();
                Accumulated--;
            }
            _valueCache.AddLast(value);
            _sum += value;
        }

        protected override void InvokeUpdateLast(double value)
        {
            _sum += value - LastAdded;
            _valueCache.Last.Value = value;
        }

        protected override void SetCurrentResult()
        {
            Result = Accumulated < Period ? double.NaN : _sum/Period;
        }
    }
}
