using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.ATCFMethod.FastAdaptiveTrendLine
{
    [Indicator(Category = "AT&CF Method", DisplayName = "Fast Adaptive Trend Line", Version = "1.0")]
    public class FastAdaptiveTrendLine : DigitalIndicatorBase, IFastAdaptiveTrendLine
    {
        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "FATL", Target = OutputTargets.Overlay, DefaultColor = Colors.Red)]
        public DataSeries Fatl { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public FastAdaptiveTrendLine() { }

        public FastAdaptiveTrendLine(DataSeries price, int countBars)
        {
            Price = price;
            CountBars = countBars;

            InitializeIndicator();
        }

        public int HasEnoughBars()
        {
            return CoefficientsCount;
        }

        private void InitializeIndicator() { }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Fatl[pos] = CalculateDigitalIndicator(Price);
            if (Price.Count > CountBars)
            {
                Fatl[CountBars] = double.NaN;
            }
        }

        protected override void SetupCoefficients()
        {
            Coefficients = new[]
            {
                0.4360409450, 0.3658689069, 0.2460452079, 0.1104506886, -0.0054034585, -0.0760367731, -0.0933058722,
                -0.0670110374, -0.0190795053, 0.0259609206, 0.0502044896, 0.0477818607, 0.0249252327, -0.0047706151,
                -0.0272432537, -0.0338917071, -0.0244141482, -0.0055774838, 0.0128149838, 0.0226522218, 0.0208778257,
                0.0100299086, -0.0036771622, -0.0136744850, -0.0160483392, -0.0108597376, -0.0016060704, 0.0069480557,
                0.0110573605, 0.0095711419, 0.0040444064, -0.0023824623, -0.0067093714, -0.0072003400, -0.0047717710,
                0.0005541115, 0.0007860160, 0.0130129076, 0.0040364019
            };
        }
    }
}
