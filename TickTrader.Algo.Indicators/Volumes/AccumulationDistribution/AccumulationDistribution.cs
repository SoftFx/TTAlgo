using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Volumes.AccumulationDistribution
{
    [Indicator(Category = "Volumes", DisplayName = "Accumulation/Distribution", Version = "1.0")]
    public class AccumulationDistribution : Indicator, IAccumulationDistribution
    {
        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "A/D", Target = OutputTargets.Window1, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Ad { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public AccumulationDistribution() { }

        public AccumulationDistribution(BarSeries bars)
        {
            Bars = bars;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {

        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Ad[pos] = (Bars.Close[pos] - Bars.Low[pos]) - (Bars.High[pos] - Bars.Close[pos]);
            if (Math.Abs(Ad[pos]) > 1e-20)
            {
                var tmp = (Bars.High[pos] - Bars.Low[pos]);
                if (Math.Abs(tmp) < 1e-20)
                {
                    Ad[pos] = 0.0;
                }
                else
                {
                    Ad[pos] /= tmp;
                    Ad[pos] *= Bars.Volume[pos];
                }
            }
            if (Bars.Count > 1)
            {
                Ad[pos] += Ad[pos + 1];
            }
        }
    }
}
