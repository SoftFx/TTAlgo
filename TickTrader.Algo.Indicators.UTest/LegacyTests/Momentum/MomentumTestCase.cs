using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

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
            Builder.MapBarInput("Close", Symbol, entity => entity.Close);
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("Period", Period);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtMomBuffer", 0);
        }
    }
}
