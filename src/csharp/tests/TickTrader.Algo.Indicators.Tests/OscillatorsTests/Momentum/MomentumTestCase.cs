using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.Momentum
{
    public class MomentumTestCase : PeriodPricesTestCase
    {
        public MomentumTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, period, 7)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Moment", 0);
        }
    }
}
