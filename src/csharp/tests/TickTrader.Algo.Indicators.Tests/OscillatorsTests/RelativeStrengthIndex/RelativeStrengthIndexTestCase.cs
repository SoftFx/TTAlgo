using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.RelativeStrengthIndex
{
    public class RelativeStrengthIndexTestCase : PeriodPricesTestCase
    {
        public RelativeStrengthIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period) : base(indicatorType, symbol, quotesPath, answerPath, 8, period, 7)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Rsi", 0);
        }
    }
}
