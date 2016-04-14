using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.ATR
{
    public class AtrTestCase : LegacyTestCase
    {
        public int InpAtrPeriod { get; set; }

        public AtrTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpAtrPeriod)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, 20)
        {
            InpAtrPeriod = inpAtrPeriod;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("InpAtrPeriod", InpAtrPeriod);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtATRBuffer", 0);
        }
    }
}
