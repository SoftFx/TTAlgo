using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.BollingerBands
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Bollinger Bands")]
    public class BollingerBands : Indicator
    {
        private IMA _sma, _ma, _p2Sma;
        private Queue<double> _smaCache, _maCache, _p2SmaCache;

        [Parameter(DefaultValue = 20, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }
        
        [Parameter(DefaultValue = 2.0, DisplayName = "Deviations")]
        public double Deviations { get; set; }

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
        public DataSeries MiddleLine { get; set; }

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries TopLine { get; set; }

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries BottomLine { get; set; }

        protected override void Init()
        {
            _smaCache = new Queue<double>();
            _maCache = new Queue<double>();
            _p2SmaCache = new Queue<double>();
            _sma = MABase.CreateMaInstance(Period, Method.Simple);
            _ma = MABase.CreateMaInstance(Period, Method.Simple);
            _p2Sma = MABase.CreateMaInstance(Period, Method.Simple);
            _sma.Init();
            _ma.Init();
            _p2Sma.Init();
        }

        protected override void Calculate()
        {
            //// ---------------------
            //if (Bars.Count == 1)
            //{
            //    Init();
            //}
            //// ---------------------
            //var smaVal = Utility.GetShiftedValue(Shift, _sma.Calculate(Bars[0]), _smaCache, Bars.Count);
            //var maVal = Utility.GetShiftedValue(Shift, _ma.Calculate(Bars[0]), _maCache, Bars.Count);
            //var appliedPrice = AppliedPrice.Calculate(Bars[0], _targetPrice);
            //var p2SmaVal = Utility.GetShiftedValue(Shift, _p2Sma.Calculate(new Bar {Close = appliedPrice*appliedPrice}),
            //    _p2SmaCache, Bars.Count);
            //var stdDev = Math.Sqrt(p2SmaVal + maVal*maVal - 2*maVal*smaVal);
            //if (Shift > 0)
            //{
            //    MiddleLine[0] = maVal;
            //    TopLine[0] = maVal + Deviations*stdDev;
            //    BottomLine[0] = maVal - Deviations*stdDev;
            //}
            //else if (Shift <= 0 && -Shift < Bars.Count)
            //{
            //    MiddleLine[-Shift] = maVal;
            //    TopLine[-Shift] = maVal + Deviations*stdDev;
            //    BottomLine[-Shift] = maVal - Deviations*stdDev;
            //}
        }
    }
}
