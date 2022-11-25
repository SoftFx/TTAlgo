using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.ATCFMethod.RangeBoundChannelIndex
{
    [Indicator(Category = "AT&CF Method", DisplayName = "Range Bound Channel Index BBands", Version = "1.0")]
    public class RangeBoundChannelIndexBBands : DigitalIndicatorBase, IRangeBoundChannelIndexBBands
    {
        private IMovAvgAlgo _stdMa, _std2Ma;
        private double _coeff, _coeff2;

        [Parameter(DefaultValue = 100, DisplayName = "Deviation Period")]
        public int DeviationPeriod { get; set; }

        [Parameter(DefaultValue = 2.0, DisplayName = "Deviation Coeff")]
        public double DeviationCoeff { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "RBCI", Target = OutputTargets.Window1, DefaultColor = Colors.Teal, Precision = 6)]
        public DataSeries Rbci { get; set; }

        [Output(DisplayName = "+2Sigma", Target = OutputTargets.Window1, DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare, Precision = 6)]
        public DataSeries Plus2Sigma { get; set; }

        [Output(DisplayName = "+Sigma", Target = OutputTargets.Window1, DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare, Precision = 6)]
        public DataSeries PlusSigma { get; set; }

        [Output(DisplayName = "Middle", Target = OutputTargets.Window1, DefaultColor = Colors.Cyan, DefaultLineStyle = LineStyles.DotsRare, Precision = 6)]
        public DataSeries Middle { get; set; }

        [Output(DisplayName = "-Sigma", Target = OutputTargets.Window1, DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare, Precision = 6)]
        public DataSeries MinusSigma { get; set; }

        [Output(DisplayName = "-2Sigma", Target = OutputTargets.Window1, DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare, Precision = 6)]
        public DataSeries Minus2Sigma { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public RangeBoundChannelIndexBBands() { }

        public RangeBoundChannelIndexBBands(DataSeries price, int deviationPeriod, double deviationCoeff)
        {
            Price = price;
            DeviationPeriod = deviationPeriod;
            DeviationCoeff = deviationCoeff;

            InitializeIndicator();
        }

        public bool HasEnoughBars(int barsCount)
        {
            return barsCount > CoefficientsCount - 1;
        }

        private void InitializeIndicator()
        {
            _stdMa = MovAvg.Create(DeviationPeriod, MovingAverageMethod.Simple);
            _std2Ma = MovAvg.Create(DeviationPeriod, MovingAverageMethod.Simple);
            _coeff = DeviationCoeff / 2;
            _coeff2 = DeviationCoeff;
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            var val = -CalculateDigitalIndicator(isNewBar, Price);

            if (Price.Count < Coefficients.Length)
            {
                Rbci[pos] = double.NaN;
                return;
            }

            Rbci[pos] = val;

            if (!isNewBar)
            {
                _stdMa.UpdateLast(val);
                _std2Ma.UpdateLast(val * val);
            }
            else
            {
                _stdMa.Add(val);
                _std2Ma.Add(val * val);
            }

            if (Price.Count < Coefficients.Length + DeviationPeriod)
            {
                Middle[pos] = double.NaN;
                PlusSigma[pos] = double.NaN;
                MinusSigma[pos] = double.NaN;
                Plus2Sigma[pos] = double.NaN;
                Minus2Sigma[pos] = double.NaN;
                return;
            }

            var average = _stdMa.Average;
            var average2 = _std2Ma.Average;

            Middle[pos] = average;
            var deviation = DeviationPeriod == 1 ? 0.0 : Math.Sqrt(average2 - average * average);

            PlusSigma[pos] = average + _coeff * deviation;
            MinusSigma[pos] = average - _coeff * deviation;
            Plus2Sigma[pos] = average + _coeff2 * deviation;
            Minus2Sigma[pos] = average - _coeff2 * deviation;
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
