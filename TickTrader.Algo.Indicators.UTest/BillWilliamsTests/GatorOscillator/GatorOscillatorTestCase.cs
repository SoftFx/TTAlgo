using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.GatorOscillator
{
    public class GatorOscillatorTestCase : MethodsPricesTestCase
    {
        public int JawsPeriod { get; protected set; }
        public int JawsShift { get; protected set; }
        public int TeethPeriod { get; protected set; }
        public int TeethShift { get; protected set; }
        public int LipsPeriod { get; protected set; }
        public int LipsShift { get; protected set; }

        public GatorOscillatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int jawsPeriod, int jawsShift, int teethPeriod, int teethShift, int lipsPeriod, int lipsShift)
            : base(indicatorType, symbol, quotesPath, answerPath, 32, 4, 7)
        {
            JawsPeriod = jawsPeriod;
            JawsShift = jawsShift;
            TeethPeriod = teethPeriod;
            TeethShift = teethShift;
            LipsPeriod = lipsPeriod;
            LipsShift = lipsShift;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("JawsPeriod", JawsPeriod);
            SetParameter("JawsShift", JawsShift);
            SetParameter("TeethPeriod", TeethPeriod);
            SetParameter("TeethShift", TeethShift);
            SetParameter("LipsPeriod", LipsPeriod);
            SetParameter("LipsShift", LipsShift);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("JawsTeethUp", 0);
            PutOutputToBuffer("JawsTeethDown", 1);
            PutOutputToBuffer("TeethLipsUp", 2);
            PutOutputToBuffer("TeethLipsDown", 3);
        }

        protected override void CheckAnswerUnit(int index, List<double>[] metaAnswer)
        {
            for (var k = 0; k < AnswerUnitSize / 8; k++)
            {
                AnswerBuffer[k][index] = double.IsNaN(AnswerBuffer[k][index])
                    ? 0
                    : AnswerBuffer[k][index];
            }
            if (!((Math.Abs(metaAnswer[2][index] - AnswerBuffer[3][index]) < Epsilon &&
                   Math.Abs(metaAnswer[3][index] - AnswerBuffer[2][index]) < Epsilon &&
                   Math.Abs(metaAnswer[0][index] - AnswerBuffer[0][index]) < Epsilon &&
                   Math.Abs(metaAnswer[1][index] - AnswerBuffer[1][index]) < Epsilon) ||
                  (Math.Abs(metaAnswer[2][index] - AnswerBuffer[3][index]) < Epsilon &&
                   Math.Abs(metaAnswer[3][index] - AnswerBuffer[2][index]) < Epsilon &&
                   Math.Abs(metaAnswer[1][index] - AnswerBuffer[0][index]) < Epsilon &&
                   Math.Abs(metaAnswer[0][index] - AnswerBuffer[1][index]) < Epsilon) ||
                  (Math.Abs(metaAnswer[2][index] - AnswerBuffer[2][index]) < Epsilon &&
                   Math.Abs(metaAnswer[3][index] - AnswerBuffer[3][index]) < Epsilon &&
                   Math.Abs(metaAnswer[1][index] - AnswerBuffer[0][index]) < Epsilon &&
                   Math.Abs(metaAnswer[0][index] - AnswerBuffer[1][index]) < Epsilon)))
            {
                base.CheckAnswerUnit(index, metaAnswer);
            }
        }
    }
}
