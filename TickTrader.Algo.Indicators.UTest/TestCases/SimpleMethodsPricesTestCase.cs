using System;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class SimpleMethodsPricesTestCase<TAns> : SimpleTestCase<TAns>
    {
        protected int MethodsCount;
        protected int PricesCount;

        public int TargetMethod { get; protected set; }
        public int TargetPrice { get; protected set; }

        protected SimpleMethodsPricesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int methodsCount, int pricesCount)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            MethodsCount = methodsCount;
            PricesCount = pricesCount;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("TargetMethod", TargetMethod);
            Builder.SetParameter("TargetPrice", TargetPrice);
        }

        protected override void LaunchTest(Action runAction)
        {
            Setup();
            for (var i = 0; i < MethodsCount; i++)
                for (var j = 0; j < PricesCount; j++)
                {
                    TargetMethod = i;
                    TargetPrice = j;
                    InvokeLaunchTest(runAction);
                }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}_{TargetMethod}_{TargetPrice}.bin");
        }
    }

    public abstract class SimpleMethodsPricesTestCase : SimpleTestCase
    {
        protected int MethodsCount;
        protected int PricesCount;

        public int TargetMethod { get; protected set; }
        public int TargetPrice { get; protected set; }
        public int Period { get; protected set; }

        protected SimpleMethodsPricesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period, int answerUnitSize, int methodsCount, int pricesCount)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            MethodsCount = methodsCount;
            PricesCount = pricesCount;
            Period = period;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("TargetMethod", TargetMethod);
            Builder.SetParameter("TargetPrice", TargetPrice);
            Builder.SetParameter("Period", Period);
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
