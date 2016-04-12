using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.RSI
{
    public class RsiTestCase : LegacyTestCase
    {
        public int InpRsiPeriod { get; set; }

        public RsiTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpRsiPeriod)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, 400)
        {
            InpRsiPeriod = inpRsiPeriod;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("InpRSIPeriod", InpRsiPeriod);
        }

        public override void InvokeFullBuildTest()
        {
            Epsilon = 36e-8;
            base.InvokeFullBuildTest();
        }

        public override void InvokeUpdateTest()
        {
            Epsilon = 1e-9;
            base.InvokeUpdateTest();
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtRSIBuffer"));
        }
    }
}
