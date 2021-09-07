using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.ATCFMethod.SlowAdaptiveTrendLine
{
    [Indicator(Category = "AT&CF Method", DisplayName = "Slow Adaptive Trend Line", Version = "1.0")]
    public class SlowAdaptiveTrendLine : DigitalIndicatorBase, ISlowAdaptiveTrendLine
    {
        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "SATL", Target = OutputTargets.Overlay, DefaultColor = Colors.DarkMagenta)]
        public DataSeries Satl { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public SlowAdaptiveTrendLine() { }

        public SlowAdaptiveTrendLine(DataSeries price, int countBars)
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
            Satl[pos] = CalculateDigitalIndicator(isNewBar, Price);
            //if (Price.Count > CountBars)
            //{
            //    Satl[CountBars] = double.NaN;
            //}
        }

        protected override void SetupCoefficients()
        {
            Coefficients = new[]
            {
                0.0982862174, 0.0975682269, 0.0961401078, 0.0940230544, 0.0912437090, 0.0878391006, 0.0838544303,
                0.0793406350, 0.0743569346, 0.0689666682, 0.0632381578, 0.0572428925, 0.0510534242, 0.0447468229,
                0.0383959950, 0.0320735368, 0.0258537721, 0.0198005183, 0.0139807863, 0.0084512448, 0.0032639979,
                -0.0015350359, -0.0059060082, -0.0098190256, -0.0132507215, -0.0161875265, -0.0186164872, -0.0205446727,
                -0.0219739146, -0.0229204861, -0.0234080863, -0.0234566315, -0.0231017777, -0.0223796900, -0.0213300463,
                -0.0199924534, -0.0184126992, -0.0166377699, -0.0147139428, -0.0126796776, -0.0105938331, -0.0084736770,
                -0.0063841850, -0.0043466731, -0.0023956944, -0.0005535180, 0.0011421469, 0.0026845693, 0.0040471369,
                0.0052380201, 0.0062194591, 0.0070340085, 0.0076266453, 0.0080376628, 0.0083037666, 0.0083694798,
                0.0082901022, 0.0080741359, 0.0077543820, 0.0073260526, 0.0068163569, 0.0062325477, 0.0056078229,
                0.0049516078, 0.0161380976
            };
        }
    }
}
