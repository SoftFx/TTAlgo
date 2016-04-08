using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.Envelopes
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Envelopes")]
    public class Envelopes : Indicator
    {
        private IMA _middleLine;
        private Queue<double> _cache;
        
        [Parameter(DefaultValue = 7, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Parameter(DefaultValue = 0.25, DisplayName = "Deviation(%)")]
        public double Deviation { get; set; }

        private Method _targetMethod;
        [Parameter(DefaultValue = 0, DisplayName = "Method")]
        public int TargetMethod
        {
            get { return (int)_targetMethod; }
            set { _targetMethod = (Method)value; }
        }

        private AppliedPrice.Target _targetPrice;
        [Parameter(DefaultValue = 0, DisplayName = "Apply To")]
        public int TargetPrice
        {
            get { return (int)_targetPrice; }
            set { _targetPrice = (AppliedPrice.Target)value; }
        }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DefaultColor = Colors.Blue)]
        public DataSeries TopLine { get; set; }

        [Output(DefaultColor = Colors.Red)]
        public DataSeries BottomLine { get; set; }

        protected override void Init()
        {
            _cache = new Queue<double>();
            _middleLine = MABase.CreateMaInstance(Period, _targetMethod);
            _middleLine.Init();
        }

        protected override void Calculate()
        {
            //// ---------------------
            //if (Bars.Count == 1)
            //{
            //    Init();
            //}
            //// ---------------------
            //var val = Utility.GetShiftedValue(Shift, _middleLine.Calculate(Bars[0]), _cache, Bars.Count);
            //if (Shift > 0)
            //{
            //    TopLine[0] = val*(1.0 + Deviation/100);
            //    BottomLine[0] = val*(1.0 - Deviation/100);
            //}
            //else if (Shift <= 0 && -Shift < Bars.Count)
            //{
            //    TopLine[-Shift] = val*(1.0 + Deviation/100);
            //    BottomLine[-Shift] = val*(1.0 - Deviation/100);
            //}
        }
    }
}
