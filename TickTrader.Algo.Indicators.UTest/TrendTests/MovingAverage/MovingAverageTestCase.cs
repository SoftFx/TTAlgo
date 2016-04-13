using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.MovingAverage
{
    public class MovingAverageTestCase : MethodsPricesTestCase
    {
        public double SmoothFactor { get; protected set; }

        public MovingAverageTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period,
            int shift, double smoothFactor = 0.0)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, 4, 7, period, shift)
        {
            SmoothFactor = smoothFactor;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("SmoothFactor", SmoothFactor);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("Average"));
        }
    }
}
