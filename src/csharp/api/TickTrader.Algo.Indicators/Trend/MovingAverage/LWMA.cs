using System.Collections.Generic;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class LWMA : MABase
    {
        private int _indexSum;
        private LinkedList<double> _valueCache;

        public LWMA(int period) : base(period)
        {
        }

        public override void Init()
        {
            base.Init();
            _indexSum = 0;
            _valueCache = new LinkedList<double>();
        }

        public override void Reset()
        {
            base.Reset();
            _indexSum = 0;
            _valueCache.Clear();
        }

        protected override void InvokeAdd(double value)
        {
            if (Accumulated <= Period)
            {
                _indexSum += Accumulated;
            }
            if (Accumulated > Period)
            {
                _valueCache.RemoveFirst();
                Accumulated--;
            }
            _valueCache.AddLast(value);
        }

        protected override void InvokeUpdateLast(double value)
        {
            _valueCache.Last.Value = value;
        }

        protected override void SetCurrentResult()
        {
            if (Accumulated < Period)
            {
                Average = double.NaN;
            }
            else
            {
                var sum = 0.0;
                var index = 1;
                foreach (var value in _valueCache)
                {
                    sum += value*index;
                    index++;
                }
                Average = sum/_indexSum;
            }
        }
    }
}
