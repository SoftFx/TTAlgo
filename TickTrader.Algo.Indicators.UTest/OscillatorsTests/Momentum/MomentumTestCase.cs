using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.Momentum
{
    public class MomentumTestCase : SimplePricesTestCase
    {
        public MomentumTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
            : base(indicatorType, symbol, quotesPath, answerPath, period, 8, 7)
        {
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("Moment"));
        }
    }
}
