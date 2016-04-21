using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Trend.ParabolicSAR
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Parabolic SAR")]
    public class ParabolicSar : Indicator
    {
        [Parameter(DefaultValue = 0.02, DisplayName = "Step")]
        public double Step { get; set; }

        [Parameter(DefaultValue = 0.2, DisplayName = "Maximum")]
        public double Maximum { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "SAR", DefaultColor = Colors.Lime, PlotType = PlotType.Points)]
        public DataSeries Sar { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public ParabolicSar() { }

        public ParabolicSar(BarSeries bars, double step, double maximum)
        {
            Bars = bars;
            Step = step;
            Maximum = maximum;

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
            
        }
    }
}
