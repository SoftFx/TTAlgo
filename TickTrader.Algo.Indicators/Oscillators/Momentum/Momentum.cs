using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Oscillators.Momentum
{
    [Indicator(Category = "Oscillators", DisplayName = "Momentum", Version = "1.0")]
    public class Momentum : Indicator, IMomentum
    {
        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "Momentum", Target = OutputTargets.Window1, DefaultColor = Colors.DodgerBlue)]
        public DataSeries Moment { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public Momentum() { }

        public Momentum(DataSeries price, int period)
        {
            Price = price;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator() { }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            if (Price.Count > pos + Period)
            {
                Moment[pos] = Price[pos]/Price[pos + Period]*100;
            }
            else
            {
                Moment[pos] = double.NaN;
            }
        }
    }
}
