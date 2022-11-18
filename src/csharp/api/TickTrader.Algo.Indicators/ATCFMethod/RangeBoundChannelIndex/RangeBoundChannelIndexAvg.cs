using System;
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
            Average = _sum / Period;
        }
    }

    [Indicator(Category = "AT&CF Method", DisplayName = "Range Bound Channel Index Avg", Version = "1.4")]
    public class RangeBoundChannelIndex : DigitalIndicatorBase, IRangeBoundChannelIndexAvg
    {
        private IMA _ma;
        private AnotherSma _stdMa, _std2Ma;
        private List<double> _calcCache;
        private DateTime _lastUpdated;

        [Parameter(DefaultValue = 18, DisplayName = "STD")]
        public int Std { get; set; }

        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Parameter(DefaultValue = CalcFrequency.M1, DisplayName = "Calculation Frequency")]
        public CalcFrequency Frequency { get; set; }

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

        public RangeBoundChannelIndex(DataSeries price, int std, int countBars, CalcFrequency frequency)
        {
            Price = price;
            Std = std;
            CountBars = countBars;
            Frequency = frequency;

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
            _calcCache = new List<double>(CountBars + 1);
            _lastUpdated = DateTime.MinValue;

            _stdMa = new AnotherSma(Std);
            _std2Ma = new AnotherSma(Std);
            _stdMa.Init();
            _std2Ma.Init();
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
                _stdMa.UpdateLast(val);
                _std2Ma.UpdateLast(val * val);
                _calcCache[_calcCache.Count - 1] = val;
            }
            else
            {
                _ma.Add(val);
                _stdMa.Add(val);
                _std2Ma.Add(val * val);
                if (_calcCache.Count == CountBars)
                    _calcCache.RemoveAt(0);
                _calcCache.Add(val);
            }

            var tmp = (_std2Ma.Sum - _stdMa.Average * _stdMa.Sum) / (Std - 1);

            var deviation = Math.Sqrt(tmp);

            UpperBound[pos] = deviation;
            LowerBound[pos] = -deviation;
            UpperBound2[pos] = 2 * deviation;
            LowerBound2[pos] = -2 * deviation;

            if (!isNewBar)
            {
                switch (Frequency)
                {
                    case CalcFrequency.EveryTick:
                        break;
                    case CalcFrequency.EveryBar:
                        return;
                    case CalcFrequency.S5:
                        if (Now - _lastUpdated < TimeSpan.FromSeconds(5))
                            return;
                        break;
                    case CalcFrequency.M1:
                        if (Now - _lastUpdated < TimeSpan.FromMinutes(1))
                            return;
                        break;
                }
            }

            _lastUpdated = Now;

            if (Price.Count > CountBars)
            {
                Rbci[CountBars] = double.NaN;
                UpperBound[CountBars] = double.NaN;
                LowerBound[CountBars] = double.NaN;
                UpperBound2[CountBars] = double.NaN;
                LowerBound2[CountBars] = double.NaN;
            }

            for (var i = _calcCache.Count - 1; i >= 0; i--)
            {
                var rbci = _ma.Average - _calcCache[_calcCache.Count - (pos + i) - 1];
                Rbci[pos + i] = rbci;
            }

            // Old calculation logic for values lying inside (CountBars, CountBars + Std) interval
            //var stdMa = new AnotherSma(Std);
            //var std2Ma = new AnotherSma(Std);
            //stdMa.Init();
            //std2Ma.Init();
            //pos = Math.Min(Price.Count - 1, CountBars - 1);
            //for (var i = 0; i < Math.Min(pos, Std); i++)
            //{
            //    var rbci = Rbci[pos - i];
            //    stdMa.Add(rbci);
            //    std2Ma.Add(rbci * rbci);
            //    var tmp2 = (std2Ma.Sum - stdMa.Average * stdMa.Average * Std) / (Std - 1);

            //    var deviation2 = Math.Sqrt(tmp2);

            //    UpperBound[pos - i] = deviation2;
            //    LowerBound[pos - i] = -deviation2;
            //    UpperBound2[pos - i] = 2 * deviation2;
            //    LowerBound2[pos - i] = -2 * deviation2;
            //}
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