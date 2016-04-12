using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.CommodityChannelIndex
{
    public class CommodityChannelIndexTestCase : SimplePricesTestCase
    {
        public CommodityChannelIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period) : base(indicatorType, symbol, quotesPath, answerPath, period, 8, 7)
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
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("Cci"));
        }
    }
}
