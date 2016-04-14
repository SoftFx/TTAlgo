using System;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class PeriodPricesTestCase<TAns> : PeriodTestCase<TAns>
    {
        protected int PricesCount;

        public int TargetPrice { get; protected set; }

        protected PeriodPricesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int period, int pricesCount)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize, period)
        {
            PricesCount = pricesCount;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("TargetPrice", TargetPrice);
        }

        protected override void LaunchTest(Action runAction)
        {
            for (var i = 0; i < PricesCount; i++)
            {
                TargetPrice = i;
                Setup();
                InvokeLaunchTest(runAction);
            }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}_{TargetPrice}.bin");
        }
    }

    public abstract class PeriodPricesTestCase : PeriodTestCase
    {
        protected int PricesCount;

        public int TargetPrice { get; protected set; }

        protected PeriodPricesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int period, int pricesCount)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize, period)
        {
            PricesCount = pricesCount;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("TargetPrice", TargetPrice);
        }

        protected override void LaunchTest(Action runAction)
        {
            for (var i = 0; i < PricesCount; i++)
            {
                TargetPrice = i;
                Setup();
                InvokeLaunchTest(runAction);
            }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}_{TargetPrice}.bin");
        }
    }
}
