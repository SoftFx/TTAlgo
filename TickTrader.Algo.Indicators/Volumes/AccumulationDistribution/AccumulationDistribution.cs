using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Volumes.AccumulationDistribution
{
    [Indicator(Category = "Volumes", DisplayName = "Volumes/Accumulation/Distribution")]
    public class AccumulationDistribution : Indicator
    {
        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "A/D", DefaultColor = Colors.LightSeaGreen)]
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
            
        }
    }
}
