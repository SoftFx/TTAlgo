using System;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal readonly struct MovAvgArgs
    {
        public MovingAverageMethod Method { get; }

        public int Period { get; }

        public double SmoothFactor { get; }


        public MovAvgArgs(MovingAverageMethod method, int period, double smoothFactor)
        {
            Method = method;
            Period = period;
            SmoothFactor = smoothFactor;
        }
    }


    internal interface IMovAvgAlgo
    {
        MovAvgArgs Args { get; }

        double Average { get; }


        void Reset();

        void Add(double value);

        void UpdateLast(double value);
    }


    internal static class MovAvg
    {
        public static IMovAvgAlgo Create(int period, MovingAverageMethod method, double smoothFactor = double.NaN)
        {
            if (double.IsNaN(smoothFactor))
            {
                smoothFactor = 2.0 / (period + 1);
            }

            var args = new MovAvgArgs(method, period, smoothFactor);
            IMovAvgAlgo algo = default;
            switch (method)
            {
                case MovingAverageMethod.Simple: algo = new SMA2(args); break;
                case MovingAverageMethod.Exponential: algo = new EMA2(args); break;
                case MovingAverageMethod.Smoothed: algo = new SMMA2(args); break;
                case MovingAverageMethod.LinearWeighted: algo = new LWMA2(args); break;
                // Design decision: Should behave like EMA with a user-defined SmoothFactor
                case MovingAverageMethod.CustomExponential: algo = new EMA2(args); break;
                case MovingAverageMethod.Triangular: algo = new TriMA2(args); break;
                default: throw new ArgumentException("Unknown Moving Average method.");
            }

            algo.Reset(); // init
            return algo;
        }
    }
}
