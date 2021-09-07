using System;

namespace TickTrader.Algo.Indicators.Tests.TestCases
{
    public abstract class PeriodTestCase<TAns>  : SimpleTestCase<TAns>
    {
        public int Period { get; protected set; }

        protected PeriodTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int period) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            Period = period;
        }

        protected override void SetupParameters()
        {
            SetParameter("Period", Period);
        }
    }

    public abstract class PeriodTestCase : SimpleTestCase
    {
        public int Period { get; protected set; }

        protected PeriodTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int period) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            Period = period;
        }

        protected override void SetupParameters()
        {
            SetParameter("Period", Period);
        }
    }
}
