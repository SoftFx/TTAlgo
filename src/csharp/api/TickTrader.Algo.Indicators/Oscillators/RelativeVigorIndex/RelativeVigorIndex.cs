using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Oscillators.RelativeVigorIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Relative Vigor Index", Version = "1.1")]
    public class RelativeVigorIndex : Indicator, IRelativeVigorIndex
    {
        private IMovingAverage _moveTriMa, _rangeTriMa;
        private IMovAvgAlgo _rviMa, _moveMa, _rangeMa;

        [Parameter(DefaultValue = 10, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "RVI Average", Target = OutputTargets.Window1, DefaultColor = Colors.Green)]
        public DataSeries RviAverage { get; set; }

        [Output(DisplayName = "Signal", Target = OutputTargets.Window1, DefaultColor = Colors.Red)]
        public DataSeries Signal { get; set; }

        public int LastPositionChanged
        {
            get { return 0; }
        }

        public RelativeVigorIndex()
        {
        }

        public RelativeVigorIndex(BarSeries bars, int period)
        {
            Bars = bars;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _moveTriMa = Indicators.MovingAverage(Bars.Move, 4, 0, MovingAverageMethod.Triangular);
            _rangeTriMa = Indicators.MovingAverage(Bars.Range, 4, 0, MovingAverageMethod.Triangular);
            _moveMa = MovAvg.Create(Period, MovingAverageMethod.Simple);
            _rangeMa = MovAvg.Create(Period, MovingAverageMethod.Simple);
            _rviMa = MovAvg.Create(4, MovingAverageMethod.Triangular);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var i = LastPositionChanged;
            if (!double.IsNaN(_moveTriMa.Average[i]))
            {
                if (!isNewBar)
                {
                    _moveMa.UpdateLast(_moveTriMa.Average[i]);
                    _rangeMa.UpdateLast(_rangeTriMa.Average[i]);
                }
                else
                {
                    _moveMa.Add(_moveTriMa.Average[i]);
                    _rangeMa.Add(_rangeTriMa.Average[i]);
                }
            }
            if (!double.IsNaN(_rangeMa.Average) && Math.Abs(_rangeMa.Average) < 1e-12)
            {
                RviAverage[i] = _moveMa.Average*Period;
            }
            else
            {
                RviAverage[i] = _moveMa.Average/_rangeMa.Average;
            }
            if (!double.IsNaN(RviAverage[i]))
            {
                if (!isNewBar)
                {
                    _rviMa.UpdateLast(RviAverage[i]);
                }
                else
                {
                    _rviMa.Add(RviAverage[i]);
                }
            }
            Signal[i] = _rviMa.Average;
        }
    }
}
