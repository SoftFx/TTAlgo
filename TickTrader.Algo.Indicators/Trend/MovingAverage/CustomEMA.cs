using System.Collections.Generic;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class CustomEMA : MABase
    {
        private double _prev;
        private double _multiplier;
        private LinkedList<double> _valueCache;

        public double SmoothFactor { get; private set; }

        public CustomEMA(int period, double smoothFactor) : base(period)
        {
            if (double.IsNaN(smoothFactor))
            {
                smoothFactor = 2.0/(period + 1);
            }
            SmoothFactor = smoothFactor;
        }

        public override void Init()
        {
            base.Init();
            _prev = double.NaN;
            _multiplier = SmoothFactor;
            _valueCache = new LinkedList<double>();
        }

        public override void Reset()
        {
            base.Reset();
            _prev = double.NaN;
            _multiplier = SmoothFactor;
            _valueCache.Clear();
        }

        protected override void InvokeAdd(double value)
        {
            if (Accumulated < Period)
            {
                _multiplier *= 1 - SmoothFactor;
            }
            if (Accumulated > Period)
            {
                _prev -= _valueCache.First.Value*_multiplier;
                _valueCache.RemoveFirst();
                Accumulated--;
                _prev += _valueCache.First.Value*_multiplier;
            }
            _valueCache.AddLast(value);
            _prev = double.IsNaN(_prev) ? value : SmoothFactor*value + (1 - SmoothFactor)*_prev;
        }

        protected override void InvokeUpdateLast(double value)
        {
            _valueCache.Last.Value = value;
            _prev = (Accumulated == 1) ? value : _prev + SmoothFactor*value - SmoothFactor*LastAdded;
        }

        protected override void SetCurrentResult()
        {
            Average = _prev;
        }
    }
}
