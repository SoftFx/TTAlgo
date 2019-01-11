using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.ATCFMethod.ReferenceFastTrendLine
{
    [Indicator(Category = "AT&CF Method", DisplayName = "Reference Fast Trend Line", Version = "1.0")]
    public class ReferenceFastTrendLine : DigitalIndicatorBase, IReferenceFastTrendLine
    {
        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "RFTL", Target = OutputTargets.Overlay, DefaultColor = Colors.Blue)]
        public DataSeries Rftl { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public ReferenceFastTrendLine() { }

        public ReferenceFastTrendLine(DataSeries price, int countBars)
        {
            Price = price;
            CountBars = countBars;

            InitializeIndicator();
        }

        public bool HasEnoughBars(int barsCount)
        {
            return barsCount > CoefficientsCount - 1;
        }

        private void InitializeIndicator() { }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Rftl[pos] = CalculateDigitalIndicator(Price);
            if (Price.Count > CountBars)
            {
                Rftl[CountBars] = double.NaN;
            }
        }

        protected override void SetupCoefficients()
        {
            Coefficients = new[]
            {
                -0.0025097319, 0.0513007762, 0.1142800493, 0.1699342860, 0.2025269304, 0.2025269304, 0.1699342860,
                0.1142800493, 0.0513007762, -0.0025097319, -0.0353166244, -0.0433375629, -0.0311244617, -0.0088618137,
                0.0120580088, 0.0233183633, 0.0221931304, 0.0115769653, -0.0022157966, -0.0126536111, -0.0157416029,
                -0.0113395830, -0.0025905610, 0.0059521459, 0.0105212252, 0.0096970755, 0.0046585685, -0.0017079230,
                -0.0063513565, -0.0074539350, -0.0050439973, -0.0007459678, 0.0032271474, 0.0051357867, 0.0044454862,
                0.0018784961, -0.0011065767, -0.0031162862, -0.0033443253, -0.0022163335, 0.0002573669, 0.0003650790,
                0.0060440751, 0.0018747783
            };
        }
    }
}
