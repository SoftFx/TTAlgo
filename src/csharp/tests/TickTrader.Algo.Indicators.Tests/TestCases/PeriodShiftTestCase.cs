using System;

namespace TickTrader.Algo.Indicators.Tests.TestCases
{
    public abstract class PeriodShiftTestCase<TAns> : PeriodTestCase<TAns>
    {
        public int Shift { get; protected set; }

        protected PeriodShiftTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int period, int shift)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize, period)
        {
            Shift = shift;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("Shift", Shift);
        }
    }

    public abstract class PeriodShiftTestCase : PeriodTestCase
    {
        public int Shift { get; protected set; }

        protected PeriodShiftTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int period, int shift)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize, period)
        {
            Shift = shift;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("Shift", Shift);
        }
    }
}
