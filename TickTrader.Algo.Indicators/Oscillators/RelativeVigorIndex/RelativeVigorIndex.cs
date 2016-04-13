using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Oscillators.RelativeVigorIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Relative Vigor Index")]
    public class RelativeVigorIndex : Indicator
    {
        private List<double> Ma { get; set; }
        private List<double> Ra { get; set; }

        [Parameter(DefaultValue = 10, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "RVI Average", DefaultColor = Colors.Green)]
        public DataSeries RviAverage { get; set; }

        [Output(DisplayName = "Signal", DefaultColor = Colors.Red)]
        public DataSeries Signal { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public RelativeVigorIndex() { }

        public RelativeVigorIndex(DataSeries<Bar> bars, int period)
        {
            Bars = bars;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            Ma = new List<double>();
            Ra = new List<double>();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var i = LastPositionChanged;
            var cnt = Bars.Count - 1;
            if (Bars.Count > i + 3)
            {
                Ma.Add((Bars[i].Close - Bars[i].Open) + 2*(Bars[i + 1].Close - Bars[i + 1].Open) +
                        2*(Bars[i + 2].Close - Bars[i + 2].Open) + (Bars[i + 3].Close - Bars[i + 3].Open));
                Ra.Add((Bars[i].High - Bars[i].Low) + 2*(Bars[i + 1].High - Bars[i + 1].Low) +
                        2*(Bars[i + 2].High - Bars[i + 2].Low) + (Bars[i + 3].High - Bars[i + 3].Low));
                RviAverage[i] = ((Ma[cnt] + Ma[cnt - 1] + Ma[cnt - 2] + Ma[cnt - 3])/
                                 (Ra[cnt] + Ra[cnt - 1] + Ra[cnt - 2] + Ra[cnt - 3]));
                Signal[i] = (RviAverage[i] + 2*RviAverage[i + 1] + 2*RviAverage[i + 2] + RviAverage[i + 3])/6;
            }
            else
            {
                Ma.Add(double.NaN);
                Ra.Add(double.NaN);
                RviAverage[i] = double.NaN;
                Signal[i] = double.NaN;
            }
        }
    }
}
