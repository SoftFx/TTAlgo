using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.CommodityChannelIndex
{
    public class CommodityChannelIndexTestCase : PeriodPricesTestCase
    {
        public CommodityChannelIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period) : base(indicatorType, symbol, quotesPath, answerPath, 8, period, 7)
        {

        }

        public override void InvokeFullBuildTest()
        {
            Epsilon = 56e-10;
            base.InvokeFullBuildTest();
        }

        public override void InvokeUpdateTest()
        {
            Epsilon = 92e-10;
            base.InvokeUpdateTest();
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Cci", 0);
        }
    }
}
