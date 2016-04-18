using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.MACD
{
    public class MacdTestCase : LegacyTestCase
    {
        public int InpFastEma { get; set; }
        public int InpSlowEma { get; set; }
        public int InpSignalSma { get; set; }

        public MacdTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpFastEma,
            int inpSlowEma, int inpSignalSma) : base(indicatorType, symbol, quotesPath, answerPath, 16, 250)
        {
            InpFastEma = inpFastEma;
            InpSlowEma = inpSlowEma;
            InpSignalSma = inpSignalSma;
        }

        protected override void SetupInput()
        {
            Builder.MapBarInput("Close", Symbol, entity => entity.Close);
        }

        protected override void SetupParameters()
        {
            SetParameter("InpFastEMA", InpFastEma);
            SetParameter("InpSlowEMA", InpSlowEma);
            SetParameter("InpSignalSMA", InpSignalSma);
        }

        public override void InvokeFullBuildTest()
        {
            Epsilon = 71e-10;
            base.InvokeFullBuildTest();
        }

        public override void InvokeUpdateTest()
        {
            Epsilon = 1e-9;
            base.InvokeUpdateTest();
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtMacdBuffer", 0);
            PutOutputToBuffer("ExtSignalBuffer", 1);
        }
    }
}
