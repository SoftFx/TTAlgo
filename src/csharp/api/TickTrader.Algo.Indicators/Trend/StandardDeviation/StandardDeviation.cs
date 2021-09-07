using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.StandardDeviation
{
    [Indicator(Category = "Trend", DisplayName = "Standard Deviation", Version = "1.0")]
    public class StandardDeviation : Indicator, IStandardDeviation
    {
        private IMovingAverage _sma, _ma;
        private IMA _p2Sma;
        private IShift _p2Shifter;
        
        [Parameter(DefaultValue = 20, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Parameter(DefaultValue = MovingAverageMethod.Simple, DisplayName = "Method")]
        public MovingAverageMethod TargetMethod { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "StdDev", Target = OutputTargets.Window1, DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries StdDev { get; set; }

        public int LastPositionChanged { get { return _p2Shifter.Position; } }

        public StandardDeviation() { }

        public StandardDeviation(DataSeries price, int period, int shift, MovingAverageMethod targetMethod = MovingAverageMethod.Simple)
        {
            Price = price;
            Period = period;
            Shift = shift;
            TargetMethod = targetMethod;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _sma = Indicators.MovingAverage(Price, Period, Shift);
            _ma = Indicators.MovingAverage(Price, Period, Shift, TargetMethod);
            _p2Sma = MABase.CreateMaInstance(Period, MovingAverageMethod.Simple);
            _p2Sma.Init();
            _p2Shifter = new SimpleShifter(Shift);
            _p2Shifter.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            if (!isNewBar)
            {
                _p2Sma.UpdateLast(Price[0]*Price[0]);
                _p2Shifter.UpdateLast(_p2Sma.Average);
            }
            else
            {
                _p2Sma.Add(Price[0]*Price[0]);
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
