using System;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class PricesTestCase<TAns> : SimpleTestCase<TAns>
    {
        protected int PricesCount;

        public int TargetPrice { get; protected set; }

        protected PricesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int pricesCount) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            PricesCount = pricesCount;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("TargetPrice", TargetPrice);
        }

        protected override void LaunchTest(Action runAction)
        {
            Setup();
            for (var i = 0; i < PricesCount; i++)
            {
                TargetPrice = i;
                InvokeLaunchTest(runAction);
            }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}_{TargetPrice}.bin");
        }
    }

    public abstract class PricesTestCase : SimpleTestCase
    {
        protected int PricesCount;

        public int TargetPrice { get; protected set; }
        public int Period { get; protected set; }
        public int Shift { get; protected set; }

        protected PricesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int pricesCount, int period, int shift)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            PricesCount = pricesCount;
            Period = period;
            Shift = shift;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("TargetPrice", TargetPrice);
            Builder.SetParameter("Period", Period);
            Builder.SetParameter("Shift", Shift);
        }

        protected override void LaunchTest(Action runAction)
        {
            Setup();
            for (var i = 0; i < PricesCount; i++)
            {
                TargetPrice = i;
                InvokeLaunchTest(runAction);
            }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}_{TargetPrice}.bin");
        }
    }
}
