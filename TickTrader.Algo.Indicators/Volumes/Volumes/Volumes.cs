using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Volumes.Volumes
{
    [Indicator(Category = "Volumes", DisplayName = "Volumes/Volumes")]
    public class Volumes : Indicator
    {
        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Value Up", DefaultColor = Colors.Green)]
        public DataSeries ValueUp { get; set; }

        [Output(DisplayName = "Value Down", DefaultColor = Colors.Red)]
        public DataSeries ValueDown { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public Volumes() { }

        public Volumes(BarSeries bars)
        {
            Bars = bars;

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
            if (Bars.Count > 1)
            {
                if (Bars.Volume[pos] > Bars.Volume[pos + 1])
                {
                    ValueUp[pos] = Bars.Volume[pos];
                    ValueDown[pos] = double.NaN;
                }
                else
                {
                    ValueUp[pos] = double.NaN;
                    ValueDown[pos] = Bars.Volume[pos];
                }
            }
            else
            {
                ValueUp[pos] = Bars.Volume[pos];
                ValueDown[pos] = double.NaN;
            }
        }
    }
}
