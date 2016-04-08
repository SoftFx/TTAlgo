using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.BillWilliams.Alligator
{
    [Indicator(IsOverlay = true, Category = "Bill Williams", DisplayName = "Bill Williams/Alligator")]
    public class Alligator : Indicator
    {
        private IMA _jaws;
        private Queue<double> _jawsCache;
        private IMA _teeth;
        private Queue<double> _teethCache;
        private IMA _lips;
        private Queue<double> _lipsCache;

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

        private Method _targetMethod;

        [Parameter(DefaultValue = 2, DisplayName = "Method")]
        public int TargetMethod
        {
            get { return (int) _targetMethod; }
            set { _targetMethod = (Method) value; }
        }

        private AppliedPrice.Target _targetPrice;

        [Parameter(DefaultValue = 4, DisplayName = "Apply To")]
        public int TargetPrice
        {
            get { return (int) _targetPrice; }
            set { _targetPrice = (AppliedPrice.Target) value; }
        }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DefaultColor = Colors.Blue)]
        public DataSeries Jaws { get; set; }

        [Output(DefaultColor = Colors.Red)]
        public DataSeries Teeth { get; set; }

        [Output(DefaultColor = Colors.Green)]
        public DataSeries Lips { get; set; }

        private void InitMaInstance(out IMA instance, out Queue<double> cache, int period)
        {
            cache = new Queue<double>();
            instance = MABase.CreateMaInstance(period, _targetMethod);
            instance.Init();
        }

        protected override void Init()
        {
            InitMaInstance(out _jaws, out _jawsCache, JawsPeriod);
            InitMaInstance(out _teeth, out _teethCache, TeethPeriod);
            InitMaInstance(out _lips, out _lipsCache, LipsPeriod);
        }

        protected override void Calculate()
        {
            //--------------------
            if (Bars.Count == 1)
            {
                Init();
            }
            //--------------------
            //Utility.ApplyShiftedValue(Jaws, _jaws.Shift, _jaws.Calculate(Bars[0]), _jawsCache, Bars.Count);
            //Utility.ApplyShiftedValue(Teeth, _teeth.Shift, _teeth.Calculate(Bars[0]), _teethCache, Bars.Count);
            //Utility.ApplyShiftedValue(Lips, _lips.Shift, _lips.Calculate(Bars[0]), _lipsCache, Bars.Count);
        }
    }
}
