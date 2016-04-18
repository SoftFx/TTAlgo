using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.BillWilliams.GatorOscillator
{
    [Indicator(Category = "Bill Williams", DisplayName = "Bill Williams/Gator Oscillator")]
    public class GatorOscillator : Indicator
    {
        private Alligator.Alligator _alligator;

        [Parameter(DefaultValue = 13, DisplayName = "Jaws Period")]
        public int JawsPeriod { get; set; }

        [Parameter(DefaultValue = 8, DisplayName = "Jaws Shift")]
        public int JawsShift { get; set; }

        [Parameter(DefaultValue = 8, DisplayName = "Teeth Period")]
        public int TeethPeriod { get; set; }

        [Parameter(DefaultValue = 5, DisplayName = "Teeth Shift")]
        public int TeethShift { get; set; }

        [Parameter(DefaultValue = 5, DisplayName = "Lips Period")]
        public int LipsPeriod { get; set; }

        [Parameter(DefaultValue = 3, DisplayName = "Lips Shift")]
        public int LipsShift { get; set; }

        [Parameter(DefaultValue = Method.Smoothed, DisplayName = "Method")]
        public Method TargetMethod { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Median, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Gator Up", DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries ValueUp { get; set; }

        [Output(DisplayName = "Gator Down", DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries ValueDown { get; set; }

        public int LastPositionChanged { get { return _alligator.LastPositionChanged; } }

        public GatorOscillator() { }

        public GatorOscillator(DataSeries<Bar> bars, int jawsPeriod, int jawsShift, int teethPeriod, int teethShift,
            int lipsPeriod, int lipsShift, Method targetMethod = Method.Simple,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            JawsPeriod = jawsPeriod;
            JawsShift = jawsShift;
            TeethPeriod = teethPeriod;
            TeethShift = teethShift;
            LipsPeriod = lipsPeriod;
            LipsShift = lipsShift;
            TargetMethod = targetMethod;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _alligator = new Alligator.Alligator(Bars, JawsPeriod, JawsShift, TeethPeriod, TeethShift, LipsPeriod,
                LipsShift, TargetMethod, TargetPrice);
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
