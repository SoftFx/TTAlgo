using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.StandardDeviation
{
    [Indicator(Category = "Trend", DisplayName = "Trend/Standard Deviation")]
    public class StandardDeviation : Indicator
    {
        private MovingAverage.MovingAverage _sma, _ma;
        private IMA _p2Sma;
        private IShift _p2Shifter;
        
        [Parameter(DefaultValue = 20, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

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

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries StdDev { get; set; }

        public int LastPositionChanged { get { return _p2Shifter.Position; } }

        public double LastMaVal { get { return _ma.Average[LastPositionChanged]; } }

        public StandardDeviation() { }

        public StandardDeviation(DataSeries<Bar> bars, int period, int shift, Method targetMethod = Method.Simple,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            Shift = shift;
            _targetMethod = targetMethod;
            _targetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _sma = new MovingAverage.MovingAverage(Bars, Period, Shift, Method.Simple, _targetPrice);
            _ma = new MovingAverage.MovingAverage(Bars, Period, Shift, _targetMethod, _targetPrice);
            _p2Sma = MABase.CreateMaInstance(Period, Method.Simple);
            _p2Sma.Init();
            _p2Shifter = new SimpleShifter(Shift);
            _p2Shifter.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var appliedPrice = AppliedPrice.Calculate(Bars[0], _targetPrice);
            if (IsUpdate)
            {
                _p2Sma.UpdateLast(appliedPrice*appliedPrice);
                _p2Shifter.UpdateLast(_p2Sma.Average);
            }
            else
            {
                _p2Sma.Add(appliedPrice*appliedPrice);
                _p2Shifter.Add(_p2Sma.Average);
            }
            var maVal = _ma.Average[_ma.LastPositionChanged];
            var smaVal = _sma.Average[_sma.LastPositionChanged];
            var p2SmaVal = _p2Shifter.Result;
            var res = Math.Sqrt(p2SmaVal + maVal*maVal - 2*maVal*smaVal);
            StdDev[_p2Shifter.Position] = res;
        }
    }
}
