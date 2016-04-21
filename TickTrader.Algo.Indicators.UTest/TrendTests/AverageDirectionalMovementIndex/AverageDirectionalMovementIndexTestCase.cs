using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.AverageDirectionalMovementIndex
{
    public class AverageDirectionalMovementIndexTestCase : PeriodPricesTestCase
    {
        public AverageDirectionalMovementIndexTestCase(Type indicatorType, string symbol, string quotesPath,
            string answerPath, int period) : base(indicatorType, symbol, quotesPath, answerPath, 24, period, 7)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Adx", 0);
            PutOutputToBuffer("PlusDmi", 1);
            PutOutputToBuffer("MinusDmi", 2);
        }
    }
}
