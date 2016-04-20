using System;
using TickTrader.Algo.Indicators.Utility;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Momentum
{
    public class MomentumTestCase : LegacyTestCase
    {
        public int Period { get; set; }

        public MomentumTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, 20)
        {
            Period = period;
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, AppliedPrice.Target.Close, "Close");
        }

        protected override void SetupParameters()
        {
            SetParameter("Period", Period);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtMomBuffer", 0);
        }
    }
}
