using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.HeikenAshi
{
    public class HeikenAshiTestCase : LegacyTestCase
    {
        public HeikenAshiTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 32, 35)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtLowHighBuffer", 0);
            PutOutputToBuffer("ExtHighLowBuffer", 1);
            PutOutputToBuffer("ExtOpenBuffer", 2);
            PutOutputToBuffer("ExtCloseBuffer", 3);
        }
    }
}
