using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.FATLSignal
{
    public class FatlSignalTestCase : DigitalIndicatorTestCase
    {
        public double PointSize { get; protected set; }

        public FatlSignalTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            double pointSize) : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
            PointSize = pointSize;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("PointSize", PointSize);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Up", 0);
            PutOutputToBuffer("Down", 1);
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
