using System;
using TickTrader.Algo.Indicators.Tests.Utility;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Tests.TestCases
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

        protected override void SetupParameters()
        {
            //SetParameter("TargetPrice", TargetPrice);
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, (AppliedPrice) TargetPrice);
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

    public abstract class PricesTestCase : SimpleTestCase
    {
        protected int PricesCount;

        public int TargetPrice { get; protected set; }

        protected PricesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int pricesCount) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            PricesCount = pricesCount;
        }

        protected override void SetupParameters()
        {
            //SetParameter("TargetPrice", TargetPrice);
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, (AppliedPrice)TargetPrice);
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
