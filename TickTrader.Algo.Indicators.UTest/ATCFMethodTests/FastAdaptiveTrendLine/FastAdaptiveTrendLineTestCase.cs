using System;
using TickTrader.Algo.Indicators.Utility;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.FastAdaptiveTrendLine
{
    public class FastAdaptiveTrendLineTestCase : SimpleTestCase
    {
        public FastAdaptiveTrendLineTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
        }

        protected override void SetupParameters() { }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, AppliedPrice.Target.Close);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Fatl", 0);
        }
    }
}
