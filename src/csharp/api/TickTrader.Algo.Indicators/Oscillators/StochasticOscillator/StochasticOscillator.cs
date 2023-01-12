using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.StochasticOscillator
{
    [Indicator(Category = "Oscillators", DisplayName = "Stochastic Oscillator", Version = "1.1")]
    public class StochasticOscillator : Indicator, IStochasticOscillator
    {
        private IMovAvgAlgo _dMa, _numMa, _denumMa;

        [Parameter(DefaultValue = 5, DisplayName = "%K Period")]
        public int KPeriod { get; set; }

        [Parameter(DefaultValue = 3, DisplayName = "Slowing")]
        public int Slowing { get; set; }

        [Parameter(DefaultValue = 3, DisplayName = "%D Period")]
        public int DPeriod { get; set; }

        [Parameter(DefaultValue = MovingAverageMethod.Simple, DisplayName = "Method")]
        public MovingAverageMethod TargetMethod { get; set; }

        [Parameter(DefaultValue = PriceField.LowHigh, DisplayName = "Price Field")]
        public PriceField TargetPrice { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Stoch", Target = OutputTargets.Window1, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Stoch { get; set; }

        [Output(DisplayName = "Signal", Target = OutputTargets.Window1, DefaultColor = Colors.Red, DefaultLineStyle = LineStyles.DotsRare)]
        public DataSeries Signal { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public StochasticOscillator() { }

        public StochasticOscillator(BarSeries bars, int kPeriod, int slowing, int dPeriod,
            MovingAverageMethod targetMethod = MovingAverageMethod.Simple, PriceField targetPrice = PriceField.LowHigh)
        {
            Bars = bars;
            KPeriod = kPeriod;
            Slowing = slowing;
            DPeriod = dPeriod;
            TargetMethod = targetMethod;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _dMa = MovAvg.Create(DPeriod, TargetMethod);
            _numMa = MovAvg.Create(Slowing, MovingAverageMethod.Simple);
            _denumMa = MovAvg.Create(Slowing, MovingAverageMethod.Simple);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        private void ApplyPriceField(out double num, out double denum, int pos)
        {
            switch (TargetPrice)
            {
                case PriceField.LowHigh:
                    num = Bars.Close[pos] - PeriodHelper.FindMin(Bars.Low, KPeriod);
                    denum = PeriodHelper.FindMax(Bars.High, KPeriod) - PeriodHelper.FindMin(Bars.Low, KPeriod);
                    break;
                case PriceField.CloseClose:
                    num = Bars.Close[pos] - PeriodHelper.FindMin(Bars.Close, KPeriod);
                    denum = PeriodHelper.FindMax(Bars.Close, KPeriod) - PeriodHelper.FindMin(Bars.Close, KPeriod);
                    break;
                default:
                    num = 0.0;
                    denum = 0.0;
                    break;
            }
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            if (Bars.Count < KPeriod + 2)
            {
                Stoch[pos] = double.NaN;
                Signal[pos] = double.NaN;
            }
            else
            {
                double num;
                double denum;
                ApplyPriceField(out num, out denum, pos);
                if (!isNewBar)
                {
                    _numMa.UpdateLast(num);
                    _denumMa.UpdateLast(denum);
                }
                else
                {
                    _numMa.Add(num);
                    _denumMa.Add(denum);
                }
                if (Math.Abs(_denumMa.Average) < 1e-20)
                {
                    Stoch[pos] = 100.0;
                }
                else
                {
                    Stoch[pos] = _numMa.Average/_denumMa.Average*100.0;
                }
                if (!double.IsNaN(Stoch[pos]))
                {
                    if (!isNewBar)
                    {
                        _dMa.UpdateLast(Stoch[pos]);
                    }
                    else
                    {
                        _dMa.Add(Stoch[pos]);
                    }
                }
                Signal[pos] = _dMa.Average;
            }
        }
    }
}
