using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.MarketFacilitationIndex
{
    public class MarketFacilitationIndexTestCase : SimpleTestCase
    {
        public MarketFacilitationIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 32)
        {
        }

        protected override void SetupParameters() { }

        protected override void GetOutput()
        {
            PutOutputToBuffer("MfiUpVolumeUp", 0);
            PutOutputToBuffer("MfiDownVolumeDown", 1);
            PutOutputToBuffer("MfiUpVolumeDown", 2);
            PutOutputToBuffer("MfiDownVolumeUp", 3);
        }
    }
}
