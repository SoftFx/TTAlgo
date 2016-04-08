using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.MACD
{
    public class MacdTestCase : LegacyTestCase
    {
        public int InpFastEma { get; set; }
        public int InpSlowEma { get; set; }
        public int InpSignalSma { get; set; }

        public MacdTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpFastEma,
            int inpSlowEma, int inpSignalSma) : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
            InpFastEma = inpFastEma;
            InpSlowEma = inpSlowEma;
            InpSignalSma = inpSignalSma;
        }

        protected override void SetupInput()
        {
            base.SetupInput();
            Builder.MapBarInput("Close", Symbol, entity => entity.Close);
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("InpFastEMA", InpFastEma);
            Builder.SetParameter("InpSlowEMA", InpSlowEma);
            Builder.SetParameter("InpSignalSMA", InpSignalSma);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtMacdBuffer"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("ExtSignalBuffer"));
        }
    }
}
