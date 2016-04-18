using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.RelativeStrengthIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Relative Strength Index")]
    public class RelativeStrengthIndex : Indicator
    {
        private IMA _uMa, _dMa;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "RSI", DefaultColor = Colors.DodgerBlue)]
        public DataSeries Rsi { get; set; }

        public int LastPositionChanged
        {
            get { return 0; }
        }

        public RelativeStrengthIndex()
        {
        }

        public RelativeStrengthIndex(DataSeries<Bar> bars, int period,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _uMa = MABase.CreateMaInstance(Period, Method.Exponential, 1.0/Period);
            _uMa.Init();
            _dMa = MABase.CreateMaInstance(Period, Method.Exponential, 1.0/Period);
            _dMa.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var u = 0.0;
            var d = 0.0;
            if (Bars.Count == Period + 1)
            {
                for (var i = 0; i < Period; i++)
                {
                    var curAppliedPrice = AppliedPrice.Calculate(Bars[i], TargetPrice);
                    var prevAppliedPrice = AppliedPrice.Calculate(Bars[i + 1], TargetPrice);
                    u += curAppliedPrice > prevAppliedPrice ? curAppliedPrice - prevAppliedPrice : 0.0;
                    d += curAppliedPrice < prevAppliedPrice ? prevAppliedPrice - curAppliedPrice : 0.0;
                }
                u /= Period;
                d /= Period;
            }
            if (Bars.Count > Period + 1)
            {
                var curAppliedPrice = AppliedPrice.Calculate(Bars[0], TargetPrice);
                var prevAppliedPrice = AppliedPrice.Calculate(Bars[1], TargetPrice);
                u = curAppliedPrice > prevAppliedPrice ? curAppliedPrice - prevAppliedPrice : 0.0;
                d = curAppliedPrice < prevAppliedPrice ? prevAppliedPrice - curAppliedPrice : 0.0;
            }
            if (Bars.Count < Period + 1)
            {
                Rsi[pos] = double.NaN;
            }
            else
            {
                if (IsUpdate)
                {
                    _uMa.UpdateLast(u);
                    _dMa.UpdateLast(d);
                }
                else
                {
                    _uMa.Add(u);
                    _dMa.Add(d);
                }
                if (!double.IsNaN(_uMa.Average) && !double.IsNaN(_dMa.Average) && Math.Abs(_uMa.Average) < 1e-12 &&
                    Math.Abs(_dMa.Average) < 1e-12)
                {
                    Rsi[pos] = 50.0;
                }
                else
                {
                    Rsi[pos] = 100.0*_uMa.Average/(_uMa.Average + _dMa.Average);
                }
            }
        }
    }
}
