using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.DeMarker
{
    public class DeMarkerTestCase : PeriodTestCase
    {
        public DeMarkerTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, period)
        {
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("DeMark"));
        }
    }
}
