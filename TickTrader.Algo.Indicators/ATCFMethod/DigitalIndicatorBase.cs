using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod
{
    public abstract class DigitalIndicatorBase : Indicator
    {
        protected double[] Coefficients;
        protected LinkedList<double> SeriesCache;

        public int CoefficientsCount { get { return Coefficients.Length; } }

        protected DigitalIndicatorBase()
        {
            SetupIndicator();
        }

        protected void SetupIndicator()
        {
            SetupCoefficients();
            SeriesCache = new LinkedList<double>();
        }

        protected abstract void SetupCoefficients();

        protected double CalculateDigitalIndicator(bool isNewBar, DataSeries series)
        {
            if (!isNewBar)
            {
                SeriesCache.First.Value = series[0];
            }
            else
            {
                SeriesCache.AddFirst(series[0]);
                if (SeriesCache.Count > Coefficients.Length)
                {
                    SeriesCache.RemoveLast();
                }
            }
            var res = 0.0;
            var i = 0;
            foreach (var val in SeriesCache)
            {
                res += Coefficients[i]*val;
                i++;
            }
            return res;
        }
    }
}
