using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.VolumesTests.MoneyFlowIndex
{
    public class MoneyFlowIndexTestCase : PeriodTestCase
    {
        public MoneyFlowIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period) : base(indicatorType, symbol, quotesPath, answerPath, 8, period)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Mfi", 0);
        }
    }
}
