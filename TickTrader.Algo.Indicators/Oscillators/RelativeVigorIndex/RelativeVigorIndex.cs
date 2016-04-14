using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.RelativeVigorIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Relative Vigor Index")]
    public class RelativeVigorIndex : Indicator
    {
        private MovingAverage _moveMa, _rangeMa;
        private IMA _rviMa;

        [Parameter(DefaultValue = 10, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "RVI Average", DefaultColor = Colors.Green)]
        public DataSeries RviAverage { get; set; }

        [Output(DisplayName = "Signal", DefaultColor = Colors.Red)]
        public DataSeries Signal { get; set; }

        public int LastPositionChanged
        {
            get { return 0; }
        }

        public RelativeVigorIndex()
        {
        }

        public RelativeVigorIndex(DataSeries<Bar> bars, int period)
        {
            Bars = bars;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _moveMa = new MovingAverage(Bars, 4, Period - 4, Method.Triangular, AppliedPrice.Target.Move);
            _rangeMa = new MovingAverage(Bars, 4, Period - 4, Method.Triangular, AppliedPrice.Target.Range);
            _rviMa = MABase.CreateMaInstance(4, Method.Triangular);
            _rviMa.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var i = LastPositionChanged;
            RviAverage[i] = ((_moveMa.Average[i] + _moveMa.Average[i + 1] + _moveMa.Average[i + 2] +
                              _moveMa.Average[i + 3])/
                             (_rangeMa.Average[i] + _rangeMa.Average[i + 1] + _rangeMa.Average[i + 2] +
                              _rangeMa.Average[i + 3]));
            if (!double.IsNaN(RviAverage[i]))
            {
                if (IsUpdate)
                {
                    _rviMa.UpdateLast(RviAverage[i]);
                }
                else
                {
                    _rviMa.Add(RviAverage[i]);
                }
            }
            Signal[i] = _rviMa.Average;
        }
    }
}
