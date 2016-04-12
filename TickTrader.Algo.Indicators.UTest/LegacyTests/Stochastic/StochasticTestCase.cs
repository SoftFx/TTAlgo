using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Stochastic
{
    public class StochasticTestCase : LegacyTestCase
    {
        public int InpKPeriod { get; set; }
        public int InpDPeriod { get; set; }
        public int InpSlowing { get; set; }

        public StochasticTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int inpKPeriod, int inpDPeriod, int inpSlowing)
            : base(indicatorType, symbol, quotesPath, answerPath, 16, 15)
        {
            InpKPeriod = inpKPeriod;
            InpDPeriod = inpDPeriod;
            InpSlowing = inpSlowing;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("InpKPeriod", InpKPeriod);
            Builder.SetParameter("InpDPeriod", InpDPeriod);
            Builder.SetParameter("InpSlowing", InpSlowing);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtMainBuffer"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("ExtSignalBuffer"));
        }
    }
}
