using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.RelativeVigorIndex
{
    public class RelativeVigorIndexTestCase : PeriodTestCase
    {
        public RelativeVigorIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
            : base(indicatorType, symbol, quotesPath, answerPath, 16, period)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("RviAverage", 0);
            PutOutputToBuffer("Signal", 1);
        }
    }
}
