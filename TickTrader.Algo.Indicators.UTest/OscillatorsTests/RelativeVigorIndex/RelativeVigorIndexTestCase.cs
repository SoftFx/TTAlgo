using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.RelativeVigorIndex
{
    public class RelativeVigorIndexTestCase : PeriodTestCase
    {
        public RelativeVigorIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
            : base(indicatorType, symbol, quotesPath, answerPath, 16, period)
        {
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("RviAverage"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("Signal"));
        }
    }
}
