using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.StandardDeviation
{
    public class StandardDeviationTestCase : PeriodShiftMethodsPricesTestCase
    {
        public StandardDeviationTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period, int shift) : base(indicatorType, symbol, quotesPath, answerPath, 8, period, shift, 4, 7)
        {
        }

        public override void InvokeFullBuildTest()
        {
            Epsilon = 1e-9;
            base.InvokeFullBuildTest();
        }

        public override void InvokeUpdateTest()
        {
            Epsilon = 3e-9;
            base.InvokeUpdateTest();
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("StdDev", 0);
        }
    }
}
