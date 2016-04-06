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

        internal static IMA CreateMAInstance(int period, int shift, Method targetMethod, AppliedPrice.Target targetPrice,
            double smoothFactor = double.NaN)
        {
            IMA instance = null;
            if (double.IsNaN(smoothFactor))
            {
                smoothFactor = 2.0/(period + 1);
            }
            switch (targetMethod)
            {
                case Method.Simple:
                    instance = new SMA(period, shift, targetPrice);
                    break;
                case Method.Exponential:
                    instance = new EMA(period, shift, targetPrice);
                    break;
                case Method.Smoothed:
                    instance = new SMMA(period, shift, targetPrice);
                    break;
                case Method.LinearWeighted:
                    instance = new LWMA(period, shift, targetPrice);
                    break;
                case Method.CustomExponential:
                    instance = new CustomEMA(period, shift, targetPrice, smoothFactor);
                    break;
                case Method.Triangular:
                    instance = new TriMA(period, shift, targetPrice);
                    break;
            }
            return instance;
        }

        protected override void Init()
        {
            MACache = new Queue<double>();
            MAInstance = CreateMAInstance(Period, Shift, _targetMethod, _targetPrice, SmoothFactor);
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
            Utility.ApplyShiftedValue(MA, Shift, MAInstance.Calculate(Bars[0]), MACache, Bars.Count);
        }
    }
}
