using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.ForceIndex
{
    public class ForceIndexTestCase : PeriodMethodsPricesTestCase
    {
        public ForceIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, period, 4, 7)
        {
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("TargetPrice", TargetPrice);
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapBars(Builder, Symbol);
        }

        public override void InvokeFullBuildTest()
        {
            Epsilon = 11e-10;
            base.InvokeFullBuildTest();
        }

        public override void InvokeUpdateTest()
        {
            Epsilon = 11e-10;
            base.InvokeUpdateTest();
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Force", 0);
        }

        protected override void CheckAnswerUnit(int index, List<double>[] metaAnswer)
        {
            if (!(index == Period - 1 && double.IsNaN(AnswerBuffer[0][index])))
            {
                base.CheckAnswerUnit(index, metaAnswer);
            }
        }
    }
}
