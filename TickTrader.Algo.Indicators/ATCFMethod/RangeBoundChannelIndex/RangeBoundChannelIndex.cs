﻿using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.ATCFMethod.RangeBoundChannelIndex
{
    internal class AnotherSma : SMA
    {
        public double Sum => _sum;

        public AnotherSma(int period) : base(period)
        {
        }

        protected override void SetCurrentResult()
        {
            Average = _sum/Period;
        }
    }

    [Indicator(Category = "AT&CF Method", DisplayName = "Range Bound Channel Index", Version = "1.1")]
    public class RangeBoundChannelIndex : DigitalIndicatorBase, IRangeBoundChannelIndex
    {
        private IMA _ma;
        private List<double> _calcCache;

        [Parameter(DefaultValue = 18, DisplayName = "STD")]
        public int Std { get; set; }

        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "RBCI", Target = OutputTargets.Window1, DefaultColor = Colors.Teal, Precision = 6)]
        public DataSeries Rbci { get; set; }

        [Output(DisplayName = "Upper Bound", Target = OutputTargets.Window1, DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare, Precision = 6)]
        public DataSeries UpperBound { get; set; }

        [Output(DisplayName = "Lower Bound", Target = OutputTargets.Window1, DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare, Precision = 6)]
        public DataSeries LowerBound { get; set; }

        [Output(DisplayName = "Upper Bound 2", Target = OutputTargets.Window1, DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare, Precision = 6)]
        public DataSeries UpperBound2 { get; set; }

        [Output(DisplayName = "Lower Bound 2", Target = OutputTargets.Window1, DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare, Precision = 6)]
        public DataSeries LowerBound2 { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public RangeBoundChannelIndex() { }

        public RangeBoundChannelIndex(DataSeries price, int std, int countBars)
        {
            Price = price;
            Std = std;
            CountBars = countBars;

            InitializeIndicator();
        }

        public bool HasEnoughBars(int barsCount)
        {
            return barsCount > CoefficientsCount - 1;
        }

        private void InitializeIndicator()
        {
            _ma = new AnotherSma(CountBars);
            _ma.Init();
            _calcCache = new List<double>();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            var val = CalculateDigitalIndicator(isNewBar, Price);

            if (!isNewBar)
            {
                _ma.UpdateLast(val);
                _calcCache[Price.Count - 1] = val;
            }
            else
            {
                _ma.Add(val);
                _calcCache.Add(val);
            }

            if (Price.Count > CountBars)
            {
                Rbci[CountBars] = double.NaN;
                UpperBound[CountBars] = double.NaN;
                LowerBound[CountBars] = double.NaN;
                UpperBound2[CountBars] = double.NaN;
                LowerBound2[CountBars] = double.NaN;
            }

            var stdMa = new AnotherSma(Std);
            var std2Ma = new AnotherSma(Std);
            stdMa.Init();
            std2Ma.Init();

            for (var i = Math.Min(Price.Count, CountBars) - 1; i >= 0; i--)
            {
                var rbci = _ma.Average - _calcCache[Price.Count - (pos + i) - 1];
                Rbci[pos + i] = rbci;
                stdMa.Add(rbci);
                std2Ma.Add(rbci*rbci);
                var tmp = (std2Ma.Sum + stdMa.Average*stdMa.Average*Std - 2*stdMa.Average*stdMa.Sum)/(Std - 1);

                var deviation = Math.Sqrt(tmp);

                UpperBound[pos + i] = deviation;
                LowerBound[pos + i] = -deviation;
                UpperBound2[pos + i] = 2*deviation;
                LowerBound2[pos + i] = -2*deviation;
            }
        }

        protected override void SetupCoefficients()
        {
            Coefficients = new[]
            {
                -35.5241819400, -29.3339896500, -18.4277449600, -5.3418475670, 7.0231636950, 16.1762815600, 20.6566210400,
                20.3266115800, 16.2702390600, 10.3524012700, 4.5964239920, 0.5817527531, -0.9559211961, -0.2191111431,
                1.8617342810, 4.0433304300, 5.2342243280, 4.8510862920, 2.9604408870, 0.1815496232, -2.5919387010,
                -4.5358834460, -5.1808556950, -4.5422535300, -3.0671459820, -1.4310126580, -0.2740437883, 0.0260722294,
                -0.5359717954, -1.6274916400, -2.7322958560, -3.3589596820, -3.2216514550, -2.3326257940, -0.9760510577,
                0.4132650195, 1.4202166770, 1.7969987350, 1.5412722800, 0.8771442423, 0.1561848839, -0.2797065802,
                -0.2245901578, 0.3278853523, 1.1887841480, 2.0577410750, 2.6270409820, 2.6973742340, 2.2289941280,
                1.3536792430, 0.3089253193, -0.6386689841, -1.2766707670, -1.5136918450, -1.3775160780, -1.6156173970
            };
        }
    }
}
