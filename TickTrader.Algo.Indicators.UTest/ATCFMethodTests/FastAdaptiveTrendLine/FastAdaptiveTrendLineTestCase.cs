using System;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.FastAdaptiveTrendLine
{
    public class FastAdaptiveTrendLineTestCase : DigitalIndicatorTestCase
    {
        public FastAdaptiveTrendLineTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, AppliedPrice.Close);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Fatl", 0);
        }
    }
}
