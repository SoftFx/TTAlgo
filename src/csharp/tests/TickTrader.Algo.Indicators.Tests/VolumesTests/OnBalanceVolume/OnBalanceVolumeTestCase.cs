using System;
using TickTrader.Algo.Indicators.Tests.TestCases;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.VolumesTests.OnBalanceVolume
{
    public class OnBalanceVolumeTestCase : PricesTestCase
    {
        public OnBalanceVolumeTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, 7)
        {
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("TargetPrice", TargetPrice);
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapBars(Builder, Symbol);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Obv", 0);
        }
    }
}
