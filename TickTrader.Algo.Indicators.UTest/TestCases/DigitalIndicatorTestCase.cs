using System;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class DigitalIndicatorTestCase<TAns> : SimpleTestCase<TAns>
    {
        public DigitalIndicatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
        }

        protected override void SetupParameters()
        {
            SetParameter("CountBars", Quotes.Count);
        }
    }

    public abstract class DigitalIndicatorTestCase : SimpleTestCase
    {
        public DigitalIndicatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
        }

        protected override void SetupParameters()
        {
            SetParameter("CountBars", Quotes.Count);
        }
    }
}
