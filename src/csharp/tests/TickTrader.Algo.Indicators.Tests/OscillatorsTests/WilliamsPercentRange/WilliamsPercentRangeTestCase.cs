using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.WilliamsPercentRange
{
    public class WilliamsPercentRangeTestCase : PeriodTestCase
    {
        public WilliamsPercentRangeTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period) : base(indicatorType, symbol, quotesPath, answerPath, 8, period)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Wpr", 0);
        }
    }
}
