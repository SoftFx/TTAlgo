﻿using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.AverageDirectionalMovementIndex
{
    [Indicator(Category = "Trend", DisplayName = "Average Directional Movement Index", Version = "1.1")]
    public class AverageDirectionalMovementIndex : Indicator, IAverageDirectionalMovementIndex
    {
        private IMovAvgAlgo _plusMa, _minusMa, _adxMa;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Close, DisplayName = "Apply To")]
        public AppliedPrice TargetPrice { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        public DataSeries Price { get; private set; }

        [Output(DisplayName = "ADX", Target = OutputTargets.Window1, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Adx { get; set; }

        [Output(DisplayName = "+DMI", Target = OutputTargets.Window1, DefaultColor = Colors.YellowGreen, DefaultLineStyle = LineStyles.DotsRare)]
        public DataSeries PlusDmi { get; set; }

        [Output(DisplayName = "-DMI", Target = OutputTargets.Window1, DefaultColor = Colors.Wheat, DefaultLineStyle = LineStyles.DotsRare)]
        public DataSeries MinusDmi { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public AverageDirectionalMovementIndex() { }

        public AverageDirectionalMovementIndex(BarSeries bars, int period, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            Bars = bars;
            Period = period;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            Price = AppliedPriceHelper.GetDataSeries(Bars, TargetPrice);
            _plusMa = MovAvg.Create(Period, MovingAverageMethod.Exponential);
            _minusMa = MovAvg.Create(Period, MovingAverageMethod.Exponential);
            _adxMa = MovAvg.Create(Period, MovingAverageMethod.Exponential);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var plusDmi = 0.0;
            var minusDmi = 0.0;
            var pos = LastPositionChanged;
            if (Bars.Count > 1)
            {
                plusDmi = Bars.High[pos] - Bars.High[pos + 1];
                minusDmi = Bars.Low[pos + 1] - Bars.Low[pos];
                plusDmi = (plusDmi < 0) ? 0 : plusDmi;
                minusDmi = (minusDmi < 0) ? 0 : minusDmi;
                if (Math.Abs(plusDmi - minusDmi) < 1e-20)
                {
                    plusDmi = 0.0;
                    minusDmi = 0.0;
                }
                else if (plusDmi < minusDmi)
                {
                    plusDmi = 0.0;
                }
                else if (minusDmi < plusDmi)
                {
                    minusDmi = 0.0;
                }
                var tr = Math.Abs(Bars.High[pos] - Bars.Low[pos]);
                tr = Math.Max(tr, Math.Abs(Bars.High[pos] - Price[pos + 1]));
                tr = Math.Max(tr, Math.Abs(Bars.Low[pos] - Price[pos + 1]));
                if (Math.Abs(tr) < 1e-20)
                {
                    plusDmi = 0.0;
                    minusDmi = 0.0;
                }
                else
                {
                    plusDmi = plusDmi/tr*100.0;
                    minusDmi = minusDmi/tr*100.0;
                }
            }
            if (!isNewBar)
            {
                _plusMa.UpdateLast(plusDmi);
                _minusMa.UpdateLast(minusDmi);
            }
            else
            {
                _plusMa.Add(plusDmi);
                _minusMa.Add(minusDmi);
            }
            PlusDmi[pos] = _plusMa.Average;
            MinusDmi[pos] = _minusMa.Average;
            var dx = Math.Abs((PlusDmi[pos] - MinusDmi[pos])/(PlusDmi[pos] + MinusDmi[pos]))*100.0;
            if (double.IsNaN(dx))
            {
                dx = 0.0;
            }
            if (!isNewBar)
            {
                _adxMa.UpdateLast(dx);
            }
            else
            {
                _adxMa.Add(dx);
            }
            Adx[pos] = _adxMa.Average;
        }
    }
}
