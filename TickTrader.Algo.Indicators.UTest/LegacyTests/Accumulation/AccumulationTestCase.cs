using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Accumulation
{
    public class AccumulationTestCase : LegacyTestCase
    {
        public AccumulationTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, 1)
        {
        }

        protected override void SetupParameters() { }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtADBuffer", 0);
        }

        protected override void CheckAnswerUnit(int index, List<double>[] metaAnswer)
        {
            for (var k = 0; k < AnswerUnitSize / 8; k++)
            {
                AnswerBuffer[k][index] = double.IsNaN(AnswerBuffer[k][index])
                    ? 0
                    : AnswerBuffer[k][index];
                AssertX.Greater(Epsilon,
                    Math.Abs(metaAnswer[k][index] - metaAnswer[k][0] - AnswerBuffer[k][index] + AnswerBuffer[k][0]));
            }
        }
    }
}
