using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Utility
{
    public static class AppliedPrice
    {
        public enum Target
        {
            Close, Open, High, Low, Median, Typical, Weighted
        }

        public static double Calculate(Bar bar, Target target)
        {
            double res = double.NaN;

            switch (target)
            {
                case Target.Close:
                    res = bar.Close;
                    break;
                case Target.Open:
                    res = bar.Open;
                    break;
                case Target.High:
                    res = bar.High;
                    break;
                case Target.Low:
                    res = bar.Low;
                    break;
                case Target.Median:
                    res = (bar.High + bar.Low)/2;
                    break;
                case Target.Typical:
                    res = (bar.High + bar.Low + bar.Close)/3;
                    break;
                case Target.Weighted:
                    res = (bar.High + bar.Low + 2*bar.Close)/4;
                    break;
            }

            return res;
        }

        public class AppliedPriceCalculator
        {
            public Target TargetPrice { get; private set; }

            public AppliedPriceCalculator(Target targetPrice)
            {
                TargetPrice = targetPrice;
            }

            public double Calculate(Bar bar)
            {
                return AppliedPrice.Calculate(bar, TargetPrice);
            }
        }
    }
}
