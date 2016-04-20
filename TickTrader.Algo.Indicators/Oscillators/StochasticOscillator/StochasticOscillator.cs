using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Oscillators.StochasticOscillator
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Stochastic Oscillator")]
    public class StochasticOscillator : Indicator
    {
        [Parameter(DefaultValue = 5, DisplayName = "%K Period")]
        public int KPeriod { get; set; }

        [Parameter(DefaultValue = 3, DisplayName = "Slowing")]
        public int Slowing { get; set; }

        [Parameter(DefaultValue = 3, DisplayName = "%D Period")]
        public int DPeriod { get; set; }

        [Parameter(DefaultValue = Method.Simple, DisplayName = "Method")]
        public Method TargetMethod { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Stoch", DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Stoch { get; set; }

        [Output(DisplayName = "Signal", DefaultColor = Colors.Red, DefaultLineStyle = LineStyles.Lines)]
        public DataSeries Signal { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public StochasticOscillator() { }

        public StochasticOscillator(DataSeries<Bar> bars, int kPeriod, int slowing, int dPeriod,
            Method targetMethod = Method.Simple)
        {
            Bars = bars;
            KPeriod = kPeriod;
            Slowing = slowing;
            DPeriod = dPeriod;
            TargetMethod = targetMethod;

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
            throw new System.NotImplementedException();
        }
    }
}
