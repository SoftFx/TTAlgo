using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Bears
{
    public class BearsTestCase : LegacyTestCase
    {
        public int BearsPeriod { get; set; }

        public BearsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int bearsPeriod)
            : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
            BearsPeriod = bearsPeriod;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("BearsPeriod", BearsPeriod);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtBearsBuffer"));
        }
    }
}
