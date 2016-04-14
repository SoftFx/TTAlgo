using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.CCI
{
    public class CciTestCase : LegacyTestCase
    {
        public int CciPeriod { get; set; }

        public CciTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int cciPeriod)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, 40)
        {
            CciPeriod = cciPeriod;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("CCIPeriod", CciPeriod);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtCCIBuffer", 0);
        }
    }
}
