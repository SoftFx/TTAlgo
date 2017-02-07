using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.MovingAverage
{
    public class MovingAverageTestCase : PeriodShiftMethodsPricesTestCase
    {
        public double SmoothFactor { get; protected set; }

        public MovingAverageTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period,
            int shift, double smoothFactor = double.NaN)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, period, shift, 4, 7)
        {
            SmoothFactor = smoothFactor;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("SmoothFactor", SmoothFactor);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Average", 0);
        }
    }
}
