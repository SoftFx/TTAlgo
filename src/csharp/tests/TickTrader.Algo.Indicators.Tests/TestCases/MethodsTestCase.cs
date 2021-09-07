using System;

namespace TickTrader.Algo.Indicators.Tests.TestCases
{
    public abstract class MethodsTestCase<TAns> : SimpleTestCase<TAns>
    {
        protected int MethodsCount;

        public int TargetMethod { get; protected set; }

        protected MethodsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int methodsCount) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            MethodsCount = methodsCount;
        }

        protected override void SetupParameters()
        {
            SetParameter("TargetMethod", TargetMethod);
        }

        protected override void LaunchTest(Action runAction)
        {
            for (var i = 0; i < MethodsCount; i++)
            {
                TargetMethod = i;
                Setup();
                InvokeLaunchTest(runAction);
            }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}_{TargetMethod}.bin");
        }
    }

    public abstract class MethodsTestCase : SimpleTestCase
    {
        protected int MethodsCount;

        public int TargetMethod { get; protected set; }

        protected MethodsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int methodsCount) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            MethodsCount = methodsCount;
        }

        protected override void SetupParameters()
        {
            SetParameter("TargetMethod", TargetMethod);
        }

        protected override void LaunchTest(Action runAction)
        {
            for (var i = 0; i < MethodsCount; i++)
            {
                TargetMethod = i;
                Setup();
                InvokeLaunchTest(runAction);
            }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}_{TargetMethod}.bin");
        }
    }
}
