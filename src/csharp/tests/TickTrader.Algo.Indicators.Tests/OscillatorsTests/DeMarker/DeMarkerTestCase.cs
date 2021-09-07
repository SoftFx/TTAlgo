using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.DeMarker
{
    public class DeMarkerTestCase : PeriodTestCase
    {
        public DeMarkerTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, period)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("DeMark", 0);
        }
    }
}
