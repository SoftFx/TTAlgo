using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Utility;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.RangeBoundChannelIndex
{
    public class RangeBoundChannelIndexTestCase : DigitalIndicatorTestCase
    {
        public int Std { get; protected set; }
        public int CountBars { get; protected set; }

        public RangeBoundChannelIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int std, int countBars) : base(indicatorType, symbol, quotesPath, answerPath, 40)
        {
            Std = std;
            CountBars = countBars;
        }

        protected override void SetupParameters()
        {
            SetParameter("CountBars", CountBars);
            SetParameter("Std", Std);
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, AppliedPrice.Target.Close);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Rbci", 0);
            PutOutputToBuffer("UpperBound", 1);
            PutOutputToBuffer("LowerBound", 2);
            PutOutputToBuffer("UpperBound2", 3);
            PutOutputToBuffer("LowerBound2", 4);
        }

        protected override void CheckAnswerUnit(int index, List<double>[] metaAnswer)
        {
            for (var k = 0; k < AnswerUnitSize / 8; k++)
            {
                metaAnswer[k][index] = Math.Abs(metaAnswer[k][index] - int.MaxValue) < 1e-20 ? 0 : metaAnswer[k][index];
                AnswerBuffer[k][index] = double.IsNaN(AnswerBuffer[k][index])
                    ? 0
                    : AnswerBuffer[k][index];
                AssertX.Greater(Epsilon, Math.Abs(metaAnswer[k][index] - AnswerBuffer[k][index]));
            }
        }
    }
}
