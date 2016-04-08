using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Awesome
{
    public class AwesomeTestCase : LegacyTestCase
    {
        public int PeriodFast { get; set; }
        public int PeriodSlow { get; set; }

        public AwesomeTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int periodFast,
            int periodSlow) : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
            PeriodFast = periodFast;
            PeriodSlow = periodSlow;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("PeriodFast", PeriodFast);
            Builder.SetParameter("PeriodSlow", PeriodSlow);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtUpBuffer"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("ExtDnBuffer"));
        }
    }
}
