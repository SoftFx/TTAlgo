using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Oscillators.Momentum
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Momentum")]
    public class Momentum : Indicator
    {
        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "Momentum", DefaultColor = Colors.DodgerBlue)]
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

        protected override void Calculate()
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
