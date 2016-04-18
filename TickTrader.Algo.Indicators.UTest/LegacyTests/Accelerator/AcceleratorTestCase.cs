using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Accelerator
{
    public class AcceleratorTestCase : LegacyTestCase
    {
        public int PeriodFast { get; set; }
        public int PeriodSlow { get; set; }
        public int DataLimit { get; set; }

        public AcceleratorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int periodFast, int periodSlow, int dataLimit) : base(indicatorType, symbol, quotesPath, answerPath, 24, 50)
        {
            PeriodFast = periodFast;
            PeriodSlow = periodSlow;
            DataLimit = dataLimit;
        }

        protected override void SetupParameters()
        {
            SetParameter("PeriodFast", PeriodFast);
            SetParameter("PeriodSlow", PeriodSlow);
            SetParameter("DataLimit", DataLimit);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtACBuffer", 0);
            PutOutputToBuffer("ExtUpBuffer", 1);
            PutOutputToBuffer("ExtDnBuffer", 2);
        }
    }
}
