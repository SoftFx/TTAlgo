using System;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class PeriodTestCase<TAns>  : SimpleTestCase<TAns>
    {
        public int Period { get; protected set; }

        protected PeriodTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int period) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            Period = period;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("Period", Period);
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}.bin");
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

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("Period", Period);
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}.bin");
        }
    }
}
