using System;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class PeriodShiftPricesTestCase<TAns> : PeriodShiftTestCase<TAns>
    {
        protected int PricesCount;

        public int TargetPrice { get; protected set; }

        protected PeriodShiftPricesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int period, int shift, int pricesCount)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize, period, shift)
        {
            PricesCount = pricesCount;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("TargetPrice", TargetPrice);
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

    public abstract class PeriodShiftPricesTestCase : PeriodShiftTestCase
    {
        protected int PricesCount;

        public int TargetPrice { get; protected set; }

        protected PeriodShiftPricesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int period, int shift, int pricesCount)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize, period, shift)
        {
            PricesCount = pricesCount;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("TargetPrice", TargetPrice);
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
