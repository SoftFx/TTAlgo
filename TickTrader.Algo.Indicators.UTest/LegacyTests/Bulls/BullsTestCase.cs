using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Bulls
{
    public class BullsTestCase : LegacyTestCase
    {
        public int BullsPeriod { get; set; }

        public BullsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int bullsPeriod)
            : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
            BullsPeriod = bullsPeriod;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("BullsPeriod", BullsPeriod);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtBullsBuffer"));
        }
    }
}
