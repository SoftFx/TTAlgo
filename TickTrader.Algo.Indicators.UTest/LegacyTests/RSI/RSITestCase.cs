using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.RSI
{
    public class RsiTestCase : LegacyTestCase
    {
        public int InpRsiPeriod { get; set; }

        public RsiTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpRsiPeriod)
            : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
            InpRsiPeriod = inpRsiPeriod;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("InpRSIPeriod", InpRsiPeriod);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtRSIBuffer"));
        }
    }
}
