using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Moving Average")]
    public class MovingAverage : Indicator
    {
        [Parameter(DefaultValue = 7, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        private Method _targetMethod;
        [Parameter(DefaultValue = 0, DisplayName = "Method")]
        public int TargetMethod
        {
            get { return (int) _targetMethod; }
            set { _targetMethod = (Method) value; }
        }

        private AppliedPrice.Target _targetPrice;
        [Parameter(DefaultValue = 0, DisplayName = "Apply To")]
        public int TargetPrice
        {
            get { return (int) _targetPrice; }
            set { _targetPrice = (AppliedPrice.Target) value; }
        }

        [Parameter(DefaultValue = 0.25, DisplayName = "Smooth Factor(CustomEMA)")]
        public double SmoothFactor { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output]
        public DataSeries MA { get; set; }

        private IMA _maInstance;
        private Queue<double> _maCache;

        protected override void Init()
        {
            _maCache = new Queue<double>();
            _maInstance = MABase.CreateMaInstance(Period, _targetMethod, SmoothFactor);
            _maInstance.Init();
        }

        protected override void Calculate()
        {
            // should be removed when Init problem will be solved
            if (Bars.Count == 1)
            {
                //Init();
            }
            // ---------------------
            _maInstance.Add(AppliedPrice.Calculate(Bars[0], _targetPrice));
            Utility.ApplyShiftedValue(MA, Shift, _maInstance.Result, _maCache, Bars.Count);
        }
    }
}
