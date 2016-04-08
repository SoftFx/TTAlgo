using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Accelerator
{
    public class AcceleratorTestCase : LegacyTestCase
    {
        public int PeriodFast { get; set; }
        public int PeriodSlow { get; set; }
        public int DataLimit { get; set; }

        public AcceleratorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int periodFast, int periodSlow, int dataLimit) : base(indicatorType, symbol, quotesPath, answerPath, 24)
        {
            PeriodFast = periodFast;
            PeriodSlow = periodSlow;
            DataLimit = dataLimit;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("PeriodFast", PeriodFast);
            Builder.SetParameter("PeriodSlow", PeriodSlow);
            Builder.SetParameter("DataLimit", DataLimit);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtACBuffer"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("ExtUpBuffer"));
            AnswerBuffer[2] = new List<double>(Builder.GetOutput<double>("ExtDnBuffer"));
        }
    }
}
