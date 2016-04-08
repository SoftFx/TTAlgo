using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.StandardDeviation
{
    public class StandardDeviationTestCase : MethodsPricesTestCase
    {
        public StandardDeviationTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period, int shift) : base(indicatorType, symbol, quotesPath, answerPath, 8, 4, 7, period, shift)
        {
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("StdDev"));
        }
    }
}
