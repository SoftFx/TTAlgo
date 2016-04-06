using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Trend.StandardDeviation
{
    [Indicator(Category = "Trend", DisplayName = "Trend/Standard Deviation")]
    public class StandardDeviation : Indicator
    {
        private IMA _sma, _ma, _p2Sma;
        private Queue<double> _smaCache, _maCache, _p2SmaCache;
        
        [Parameter(DefaultValue = 20, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        private MovingAverage.MovingAverage.Method _targetMethod;
        [Parameter(DefaultValue = 0, DisplayName = "Method")]
        public int TargetMethod
        {
            get { return (int)_targetMethod; }
            set { _targetMethod = (MovingAverage.MovingAverage.Method)value; }
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

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries StdDev { get; set; }

        protected override void Init()
        {
            _smaCache = new Queue<double>();
            _maCache = new Queue<double>();
            _p2SmaCache = new Queue<double>();
            _sma = MovingAverage.MovingAverage.CreateMAInstance(Period, Shift, MovingAverage.MovingAverage.Method.Simple,
                _targetPrice);
            _ma = MovingAverage.MovingAverage.CreateMAInstance(Period, Shift, _targetMethod, _targetPrice);
            _p2Sma = MovingAverage.MovingAverage.CreateMAInstance(Period, Shift,
                MovingAverage.MovingAverage.Method.Simple, AppliedPrice.Target.Close);
            _sma.Init();
            _ma.Init();
            _p2Sma.Init();
        }

        protected override void Calculate()
        {
            // ---------------------
            if (Bars.Count == 1)
            {
                Init();
            }
            // ---------------------
            var smaVal = Utility.GetShiftedValue(Shift, _sma.Calculate(Bars[0]), _smaCache, Bars.Count);
            var maVal = Utility.GetShiftedValue(Shift, _ma.Calculate(Bars[0]), _maCache, Bars.Count);
            var appliedPrice = AppliedPrice.Calculate(Bars[0], _targetPrice);
            var p2SmaVal = Utility.GetShiftedValue(Shift, _p2Sma.Calculate(new Bar {Close = appliedPrice*appliedPrice}),
                _p2SmaCache, Bars.Count);
            var res = Math.Sqrt(p2SmaVal + maVal * maVal - 2 * maVal * smaVal);
            if (Shift > 0)
            {
                StdDev[0] = res;
            }
            else if (Shift <= 0 && -Shift < Bars.Count)
            {
                StdDev[-Shift] = res;
            }
        }
    }
}
