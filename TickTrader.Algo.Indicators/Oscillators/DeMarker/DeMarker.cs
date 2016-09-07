using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Oscillators.DeMarker
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/DeMarker")]
    public class DeMarker : Indicator
    {
        private IMA _smaDeMax, _smaDeMin;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "DeMarker", DefaultColor = Colors.LightSeaGreen)]
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
            _smaDeMax = MABase.CreateMaInstance(Period, Method.Simple);
            _smaDeMax.Init();
            _smaDeMin = MABase.CreateMaInstance(Period, Method.Simple);
            _smaDeMin.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var deMax = Bars.Count > 1 && Bars[0].High > Bars[1].High ? Bars[0].High - Bars[1].High : 0.0;
            var deMin = Bars.Count > 1 && Bars[0].Low < Bars[1].Low ? Bars[1].Low - Bars[0].Low : 0.0;
            if (IsUpdate)
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
