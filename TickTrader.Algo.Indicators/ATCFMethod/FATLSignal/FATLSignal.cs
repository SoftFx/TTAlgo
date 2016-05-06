using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.ATCFMethod.FATLSignal
{
    [Indicator(Category = "AT&CF Method", DisplayName = "AT&CF Method/FATLs")]
    public class FatlSignal : Indicator
    {
        [Parameter(DisplayName = "Point Size", DefaultValue = 1e-5)]
        public double PointSize { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "Up", DefaultColor = Colors.Blue)]
        public DataSeries Up { get; set; }

        [Output(DisplayName = "Down", DefaultColor = Colors.Red)]
        public DataSeries Down { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public FatlSignal() { }

        public FatlSignal(BarSeries bars, double pointSize, AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            PointSize = pointSize;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        private void InitializeIndicator() { }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            
        }
    }
}
