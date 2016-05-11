using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod
{
    public abstract class DigitalIndicatorBase : Indicator
    {
        protected double[] Coefficients;

        public int CoefficientsCount { get { return Coefficients.Length; } }

        protected DigitalIndicatorBase()
        {
            SetupIndicator();
        }

        protected void SetupIndicator()
        {
            SetupCoefficients();
        }

        protected abstract void SetupCoefficients();

        protected double CalculateDigitalIndicator(DataSeries series)
        {
            var res = 0.0;
            for (var i = 0; i < Math.Min(Coefficients.Length, series.Count); i++)
            {
                res += Coefficients[i]*series[i];
            }
            return res;
        }
    }
}
