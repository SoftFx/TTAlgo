using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Awesome
{
    public class AwesomeTestCase : LegacyTestCase
    {
        public int PeriodFast { get; set; }
        public int PeriodSlow { get; set; }

        public AwesomeTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int periodFast,
            int periodSlow) : base(indicatorType, symbol, quotesPath, answerPath, 16, 40)
        {
            PeriodFast = periodFast;
            PeriodSlow = periodSlow;
        }

        protected override void SetupParameters()
        {
            SetParameter("PeriodFast", PeriodFast);
            SetParameter("PeriodSlow", PeriodSlow);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtUpBuffer", 0);
            PutOutputToBuffer("ExtDnBuffer", 1);
        }
    }
}
