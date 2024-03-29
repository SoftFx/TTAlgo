﻿using System;
using TickTrader.Algo.Indicators.Tests.Utility;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Tests.TestCases
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

        protected override void SetupParameters()
        {
            base.SetupParameters();
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

        protected override void SetupParameters()
        {
            base.SetupParameters();
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
}
