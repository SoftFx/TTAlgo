using System;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal interface IMovAvgAlgo
    {
        void OnInit();

        void OnReset();

        void OnAdded(double value);

        void OnLastUpdated(double value);

        double Calculate();
    }


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


    internal class MovAvg : IMA
    {
        private readonly MovAvgArgs _args;
        private readonly IMovAvgAlgo _algo;


        public double Average { get; private set; }


        private MovAvg(MovAvgArgs args, IMovAvgAlgo algo)
        {
            _args = args;
            _algo = algo;
        }


        public void Init()
        {
            Average = double.NaN;
            _algo.OnInit();
        }

        public void Reset()
        {
            Average = double.NaN;
            _algo.OnReset();
        }

        public void Add(double value)
        {
            _algo.OnAdded(value);
            Average = _algo.Calculate();
        }

        public void UpdateLast(double value)
        {
            _algo.OnLastUpdated(value);
            Average = _algo.Calculate();
        }


        internal static MovAvg Create(int period, MovingAverageMethod method, double smoothFactor = double.NaN)
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
                case MovingAverageMethod.CustomExponential: algo = new CustomEMA2(args); break;
                case MovingAverageMethod.Triangular: algo = new TriMA2(args); break;
                default: throw new ArgumentException("Unknown Moving Average method.");
            }

            return new MovAvg(args, algo);
        }
    }
}
