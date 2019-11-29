using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.ATCFMethod.ReferenceSlowTrendLine
{
    [Indicator(Category = "AT&CF Method", DisplayName = "Reference Slow Trend Line", Version = "1.0")]
    public class ReferenceSlowTrendLine : DigitalIndicatorBase, IReferenceSlowTrendLine
    {
        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "RSTL", Target = OutputTargets.Overlay, DefaultColor = Colors.DeepSkyBlue)]
        public DataSeries Rstl { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public ReferenceSlowTrendLine() { }

        public ReferenceSlowTrendLine(DataSeries price, int countBars)
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

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            Rstl[pos] = CalculateDigitalIndicator(isNewBar, Price);
            if (Price.Count > CountBars)
            {
                Rstl[CountBars] = double.NaN;
            }
        }

        protected override void SetupCoefficients()
        {
            Coefficients = new[]
            {
                -0.0074151919, -0.0060698985, -0.0044979052, -0.0027054278, -0.0007031702, 0.0014951741, 0.0038713513,
                0.0064043271, 0.0090702334, 0.0118431116, 0.0146922652, 0.0175884606, 0.0204976517, 0.0233865835,
                0.0262218588, 0.0289681736, 0.0315922931, 0.0340614696, 0.0363444061, 0.0384120882, 0.0402373884,
                0.0417969735, 0.0430701377, 0.0440399188, 0.0446941124, 0.0450230100, 0.0450230100, 0.0446941124,
                0.0440399188, 0.0430701377, 0.0417969735, 0.0402373884, 0.0384120882, 0.0363444061, 0.0340614696,
                0.0315922931, 0.0289681736, 0.0262218588, 0.0233865835, 0.0204976517, 0.0175884606, 0.0146922652,
                0.0118431116, 0.0090702334, 0.0064043271, 0.0038713513, 0.0014951741, -0.0007031702, -0.0027054278,
                -0.0044979052, -0.0060698985, -0.0074151919, -0.0085278517, -0.0094111161, -0.0100658241, -0.0104994302,
                -0.0107227904, -0.0107450280, -0.0105824763, -0.0102517019, -0.0097708805, -0.0091581551, -0.0084345004,
                -0.0076214397, -0.0067401718, -0.0058083144, -0.0048528295, -0.0038816271, -0.0029244713, -0.0019911267,
                -0.0010974211, -0.0002535559, 0.0005231953, 0.0012297491, 0.0018539149, 0.0023994354, 0.0028490136,
                0.0032221429, 0.0034936183, 0.0036818974, 0.0038037944, 0.0038338964, 0.0037975350, 0.0036986051,
                0.0035521320, 0.0033559226, 0.0031224409, 0.0028550092, 0.0025688349, 0.0022682355, 0.0073925495
            };
        }
    }
}
