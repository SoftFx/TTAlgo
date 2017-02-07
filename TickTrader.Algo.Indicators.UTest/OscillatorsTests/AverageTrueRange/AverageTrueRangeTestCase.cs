using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.AverageTrueRange
{
    public class AverageTrueRangeTestCase : PeriodTestCase
    {
        public AverageTrueRangeTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period) : base(indicatorType, symbol, quotesPath, answerPath, 8, period)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Atr", 0);
        }
    }
}
