using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Bears
{
    public class BearsTestCase : LegacyTestCase
    {
        public int BearsPeriod { get; set; }

        public BearsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int bearsPeriod)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, 150)
        {
            BearsPeriod = bearsPeriod;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("BearsPeriod", BearsPeriod);
        }

        public override void InvokeFullBuildTest()
        {
            Epsilon = 43e-10;
            base.InvokeFullBuildTest();
        }

        public override void InvokeUpdateTest()
        {
            Epsilon = 1e-9;
            base.InvokeUpdateTest();
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtBearsBuffer", 0);
        }
    }
}
