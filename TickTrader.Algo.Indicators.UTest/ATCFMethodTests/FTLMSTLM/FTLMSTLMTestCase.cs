using System;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.FTLMSTLM
{
    public class FtlmStlmTestCase : DigitalIndicatorTestCase
    {
        public FtlmStlmTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, AppliedPrice.Close);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Ftlm", 0);
            PutOutputToBuffer("Stlm", 1);
        }
    }
}
