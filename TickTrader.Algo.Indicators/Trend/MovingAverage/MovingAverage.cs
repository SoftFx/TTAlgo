using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Moving Average")]
    public class MovingAverage : Indicator
    {
        public enum Method
        {
            Simple, Exponential, Smoothed, LinearWeighted
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
            }
            MAInstance.Init();
        }

        protected override void Calculate()
        {
            var res = MAInstance.Calculate(Bars[0]);
            if (MAInstance.Accumulated == Period)
            {
                if (Shift > 0)
                {
                    if (Bars.Count > Period + Shift)
                    {
                        MA[0] = MACache.Dequeue();
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
}
