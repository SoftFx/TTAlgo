using System;
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

        protected override void SetupParameters()
        {
            SetParameter("InpKPeriod", InpKPeriod);
            SetParameter("InpDPeriod", InpDPeriod);
            SetParameter("InpSlowing", InpSlowing);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtMainBuffer", 0);
            PutOutputToBuffer("ExtSignalBuffer", 1);
        }
    }
}
