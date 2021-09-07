using System;
using TickTrader.Algo.Indicators.Tests.TestCases;
using TickTrader.Algo.Indicators.Tests.Utility;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Tests.ATCFMethodTests.PerfectCommodityChannelIndex
{
    public class PerfectCommodityChannelIndexTestCase : DigitalIndicatorTestCase
    {
        public PerfectCommodityChannelIndexTestCase(Type indicatorType, string symbol, string quotesPath,
            string answerPath) : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, AppliedPrice.Close);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Pcci", 0);
        }
    }
}
