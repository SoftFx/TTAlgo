using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Moving Average")]
    public class MovingAverage : Indicator
    {
        public enum Method
        {
            Simple, Exponential, Smoothed, LinearWeighted, CustomExponential, Triangular
        }

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

        private IMA MAInstance;
        private Queue<double> MACache;

        protected override void Init()
        {
            MACache = new Queue<double>();
            switch (_targetMethod)
            {
                case Method.Simple:
                    MAInstance = new SMA(Period, Shift, _targetPrice);
                    break;
                case Method.Exponential:
                    MAInstance = new EMA(Period, Shift, _targetPrice);
                    break;
                case Method.Smoothed:
                    MAInstance = new SMMA(Period, Shift, _targetPrice);
                    break;
                case Method.LinearWeighted:
                    MAInstance = new LWMA(Period, Shift, _targetPrice);
                    break;
                case Method.CustomExponential:
                    MAInstance = new CustomEMA(Period, Shift, _targetPrice, SmoothFactor);
                    break;
                case Method.Triangular:
                    MAInstance = new TriMA(Period, Shift, _targetPrice);
                    break;
            }
            MAInstance.Init();
        }

        protected override void Calculate()
        {
            // should be removed when Init problem will be solved
            if (Bars.Count == 1)
            {
                Init();
            }
            // ---------------------
            var res = MAInstance.Calculate(Bars[0]);
            if (Shift > 0)
            {
                if (Bars.Count > Shift)
                {
                    MA[0] = MACache.Dequeue();
                }
                else
                {
                    MA[0] = double.NaN;
                }
                MACache.Enqueue(res);
            }
            else if (Shift <= 0 && -Shift < Bars.Count)
            {
                MA[-Shift] = res;
            }
        }
    }
}
