using System;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class PeriodShiftMethodsPricesTestCase<TAns> : PeriodShiftTestCase<TAns>
    {
        protected int MethodsCount;
        protected int PricesCount;

        public int TargetMethod { get; protected set; }
        public int TargetPrice { get; protected set; }

        protected PeriodShiftMethodsPricesTestCase(Type indicatorType, string symbol, string quotesPath,
            string answerPath, int answerUnitSize, int period, int shift, int methodsCount, int pricesCount)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize, period, shift)
        {
            MethodsCount = methodsCount;
            PricesCount = pricesCount;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("TargetMethod", TargetMethod);
            SetBuilderParameter("TargetPrice", TargetPrice);
        }

        protected override void LaunchTest(Action runAction)
        {
            for (var i = 0; i < MethodsCount; i++)
                for (var j = 0; j < PricesCount; j++)
                {
                    TargetMethod = i;
                    TargetPrice = j;
                    Setup();
                    InvokeLaunchTest(runAction);
                }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}_{TargetMethod}_{TargetPrice}.bin");
        }
    }

    public abstract class PeriodShiftMethodsPricesTestCase : PeriodShiftTestCase
    {
        protected int MethodsCount;
        protected int PricesCount;

        public int TargetMethod { get; protected set; }
        public int TargetPrice { get; protected set; }

        protected PeriodShiftMethodsPricesTestCase(Type indicatorType, string symbol, string quotesPath,
            string answerPath, int answerUnitSize, int period, int shift, int methodsCount, int pricesCount)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize, period, shift)
        {
            MethodsCount = methodsCount;
            PricesCount = pricesCount;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("TargetMethod", TargetMethod);
            SetBuilderParameter("TargetPrice", TargetPrice);
        }

        protected override void LaunchTest(Action runAction)
        {
            for (var i = 0; i < MethodsCount; i++)
                for (var j = 0; j < PricesCount; j++)
                {
                    TargetMethod = i;
                    TargetPrice = j;
                    Setup();
                    InvokeLaunchTest(runAction);
                }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}_{TargetMethod}_{TargetPrice}.bin");
        }
    }
}
