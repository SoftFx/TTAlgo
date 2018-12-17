using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Oscillators.RelativeStrengthIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Relative Strength Index", Version = "1.0")]
    public class RelativeStrengthIndex : Indicator
    {
        private IMA _uMa, _dMa;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "RSI", Target = OutputTargets.Window1, DefaultColor = Colors.DodgerBlue)]
        public DataSeries Rsi { get; set; }

        public int LastPositionChanged
        {
            get { return 0; }
        }

        public RelativeStrengthIndex()
        {
        }

        public RelativeStrengthIndex(DataSeries price, int period)
        {
            Price = price;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _uMa = MABase.CreateMaInstance(Period, MovingAverageMethod.Exponential, 1.0/Period);
            _uMa.Init();
            _dMa = MABase.CreateMaInstance(Period, MovingAverageMethod.Exponential, 1.0/Period);
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
            if (Price.Count == Period + 1)
            {
                for (var i = 0; i < Period; i++)
                {
                    u += Price[i] > Price[i + 1] ? Price[i] - Price[i + 1] : 0.0;
                    d += Price[i] < Price[i + 1] ? Price[i + 1] - Price[i] : 0.0;
                }
                u /= Period;
                d /= Period;
            }
            if (Price.Count > Period + 1)
            {
                u = Price[0] > Price[1] ? Price[0] - Price[1] : 0.0;
                d = Price[0] < Price[1] ? Price[1] - Price[0] : 0.0;
            }
            if (Price.Count < Period + 1)
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
