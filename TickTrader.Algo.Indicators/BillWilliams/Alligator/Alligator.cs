using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.BillWilliams.Alligator
{
    [Indicator(Category = "Bill Williams", DisplayName = "Alligator", Version = "1.0")]
    public class Alligator : Indicator, IAlligator
    {
        private IMovingAverage _jaws, _teeth, _lips;

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

        [Parameter(DefaultValue = MovingAverageMethod.Smoothed, DisplayName = "Method")]
        public MovingAverageMethod TargetMethod { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "Jaws", Target = OutputTargets.Overlay, DefaultColor = Colors.Blue)]
        public DataSeries Jaws { get; set; }

        [Output(DisplayName = "Teeth", Target = OutputTargets.Overlay, DefaultColor = Colors.Red)]
        public DataSeries Teeth { get; set; }

        [Output(DisplayName = "Lips", Target = OutputTargets.Overlay, DefaultColor = Colors.Green)]
        public DataSeries Lips { get; set; }

        public int LastPositionChanged { get { return _jaws.LastPositionChanged; } }

        public Alligator() { }

        public Alligator(DataSeries price, int jawsPeriod, int jawsShift, int teethPeriod, int teethShift,
            int lipsPeriod, int lipsShift, MovingAverageMethod targetMethod = MovingAverageMethod.Smoothed)
        {
            Price = price;
            JawsPeriod = jawsPeriod;
            JawsShift = jawsShift;
            TeethPeriod = teethPeriod;
            TeethShift = teethShift;
            LipsPeriod = lipsPeriod;
            LipsShift = lipsShift;
            TargetMethod = targetMethod;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _jaws = Indicators.MovingAverage(Price, JawsPeriod, JawsShift, TargetMethod);
            _teeth = Indicators.MovingAverage(Price, TeethPeriod, TeethShift, TargetMethod);
            _lips = Indicators.MovingAverage(Price, LipsPeriod, LipsShift, TargetMethod);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            Jaws[pos] = _jaws.Average[pos];
            Teeth[pos] = _teeth.Average[pos];
            Lips[pos] = _lips.Average[pos];
        }
    }
}
