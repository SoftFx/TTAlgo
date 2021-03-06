using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Oscillators.DeMarker
{
    [Indicator(Category = "Oscillators", DisplayName = "DeMarker", Version = "1.0")]
    public class DeMarker : Indicator, IDeMarker
    {
        private IMA _smaDeMax, _smaDeMin;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "DeMarker", Target = OutputTargets.Window1, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries DeMark { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public DeMarker() { }

        public DeMarker(BarSeries bars, int period)
        {
            Bars = bars;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _smaDeMax = MABase.CreateMaInstance(Period, MovingAverageMethod.Simple);
            _smaDeMax.Init();
            _smaDeMin = MABase.CreateMaInstance(Period, MovingAverageMethod.Simple);
            _smaDeMin.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var deMax = Bars.Count > 1 && Bars[0].High > Bars[1].High ? Bars[0].High - Bars[1].High : 0.0;
            var deMin = Bars.Count > 1 && Bars[0].Low < Bars[1].Low ? Bars[1].Low - Bars[0].Low : 0.0;
            if (!isNewBar)
            {
                _smaDeMax.UpdateLast(deMax);
                _smaDeMin.UpdateLast(deMin);
            }
            else
            {
                _smaDeMax.Add(deMax);
                _smaDeMin.Add(deMin);
            }
            DeMark[LastPositionChanged] = _smaDeMax.Average/(_smaDeMax.Average + _smaDeMin.Average);
        }
    }
}
