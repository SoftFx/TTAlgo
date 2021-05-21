using System;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal abstract class MABase : IMA
    {
        public int Period { get; private set; }

        public int Accumulated { get; protected set; }
        public double LastAdded { get; protected set; }
        public double Average { get; protected set; }

        internal MABase(int period)
        {
            Period = period;
        }

        public virtual void Init()
        {
            Accumulated = 0;
            LastAdded = double.NaN;
            Average = double.NaN;
        }

        public virtual void Reset()
        {
            Accumulated = 0;
            LastAdded = double.NaN;
            Average = double.NaN;
        }

        protected abstract void InvokeAdd(double value);
        protected abstract void InvokeUpdateLast(double value);
        protected abstract void SetCurrentResult();

        public void Add(double value)
        {
            Accumulated++;
            InvokeAdd(value);
            SetCurrentResult();
            LastAdded = value;
        }

        public void UpdateLast(double value)
        {
            if (Accumulated == 0)
            {
                throw new Exception("Last element doesn't exists.");
            }
            InvokeUpdateLast(value);
            SetCurrentResult();
            LastAdded = value;
        }

        internal static IMA CreateMaInstance(int period, MovingAverageMethod targetMethod, double smoothFactor = double.NaN)
        {
            IMA instance;
            if (double.IsNaN(smoothFactor))
            {
                smoothFactor = 2.0/(period + 1);
            }
            switch (targetMethod)
            {
                case MovingAverageMethod.Simple:
                    instance = new SMA(period);
                    break;
                case MovingAverageMethod.Exponential:
                    instance = new EMA(period, smoothFactor);
                    break;
                case MovingAverageMethod.Smoothed:
                    instance = new SMMA(period);
                    break;
                case MovingAverageMethod.LinearWeighted:
                    instance = new LWMA(period);
                    break;
                case MovingAverageMethod.CustomExponential:
                    instance = new CustomEMA(period, smoothFactor);
                    break;
                case MovingAverageMethod.Triangular:
                    instance = new TriMA(period);
                    break;
                default:
                    throw new ArgumentException("Unknown Moving Average method.");
            }
            return instance;
        }
    }
}
