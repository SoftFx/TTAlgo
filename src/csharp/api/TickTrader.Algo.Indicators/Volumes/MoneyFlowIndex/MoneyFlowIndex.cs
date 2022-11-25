using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Volumes.MoneyFlowIndex
{
    [Indicator(Category = "Volumes", DisplayName = "Money Flow Index", Version = "1.0")]
    public class MoneyFlowIndex : Indicator, IMoneyFlowIndex
    {
        private IMovAvgAlgo _positiveMa, _negativeMa;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "MFI", Target = OutputTargets.Window1, DefaultColor = Colors.DodgerBlue)]
        public DataSeries Mfi { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public MoneyFlowIndex() { }

        public MoneyFlowIndex(BarSeries bars, int period)
        {
            Bars = bars;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _positiveMa = MovAvg.Create(Period, MovingAverageMethod.Simple);
            _negativeMa = MovAvg.Create(Period, MovingAverageMethod.Simple);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            var positive = 0.0;
            var negative = 0.0;
            if (Bars.Count > 1)
            {
                if (Bars.Typical[pos] > Bars.Typical[pos + 1])
                {
                    positive = Bars.Volume[pos]*Bars.Typical[pos];
                }
                if (Bars.Typical[pos] < Bars.Typical[pos + 1])
                {
                    negative = Bars.Volume[pos]*Bars.Typical[pos];
                }
            }
            else
            {
                positive = Bars.Volume[pos]*Bars.Typical[pos];
            }
            if (!isNewBar)
            {
                _positiveMa.UpdateLast(positive);
                _negativeMa.UpdateLast(negative);
            }
            else
            {
                _positiveMa.Add(positive);
                _negativeMa.Add(negative);
            }
            if (!double.IsNaN(_negativeMa.Average) && !double.IsNaN(_positiveMa.Average))
            {
                if (Math.Abs(_negativeMa.Average) > 1e-20)
                {
                    Mfi[pos] = 100.0 - 100.0/(1.0 + _positiveMa.Average/_negativeMa.Average);
                }
                else
                {
                    Mfi[pos] = 100.0;
                }
            }
            else
            {
                Mfi[pos] = double.NaN;
            }
        }
    }
}
