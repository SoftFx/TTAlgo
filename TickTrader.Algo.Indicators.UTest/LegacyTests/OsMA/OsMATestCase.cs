using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.OsMA
{
    public class OsMaTestCase : LegacyTestCase
    {
        public int InpFastEma { get; set; }
        public int InpSlowEma { get; set; }
        public int InpSignalSma { get; set; }

        public OsMaTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpFastEma,
            int inpSlowEma, int inpSignalSma) : base(indicatorType, symbol, quotesPath, answerPath, 8, 250)
        {
            InpFastEma = inpFastEma;
            InpSlowEma = inpSlowEma;
            InpSignalSma = inpSignalSma;
        }

        protected override void SetupInput()
        {
            Builder.MapBarInput("Close", Symbol, entity => entity.Close);
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("InpFastEMA", InpFastEma);
            Builder.SetParameter("InpSlowEMA", InpSlowEma);
            Builder.SetParameter("InpSignalSMA", InpSignalSma);
        }

        public override void InvokeFullBuildTest()
        {
            Epsilon = 17e-10;
            base.InvokeFullBuildTest();
        }

        public override void InvokeUpdateTest()
        {
            Epsilon = 1e-9;
            base.InvokeUpdateTest();
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtOsmaBuffer"));
        }
    }
}
