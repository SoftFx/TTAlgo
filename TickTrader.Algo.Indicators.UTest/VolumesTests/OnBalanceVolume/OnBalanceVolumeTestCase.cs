using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.VolumesTests.OnBalanceVolume
{
    public class OnBalanceVolumeTestCase : PricesTestCase
    {
        public OnBalanceVolumeTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, 7)
        {
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Obv", 0);
        }
    }
}
