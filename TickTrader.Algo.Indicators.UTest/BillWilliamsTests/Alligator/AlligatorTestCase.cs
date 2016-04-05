using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.Alligator
{
    public class AlligatorTestCase : MethodsPricesTestCase<List<double>[]>
    {
        public int JawsPeriod { get; protected set; }
        public int JawsShift { get; protected set; }
        public int TeethPeriod { get; protected set; }
        public int TeethShift { get; protected set; }
        public int LipsPeriod { get; protected set; }
        public int LipsShift { get; protected set; }

        public AlligatorTestCase(Type indicatorType, string quotesPath, string answerPath, int jawsPeriod, int jawsShift,
            int teethPeriod, int teethShift, int lipsPeriod, int lipsShift)
            : base(indicatorType, quotesPath, answerPath, 4, 7, 24)
        {
            JawsPeriod = jawsPeriod;
            JawsShift = jawsShift;
            TeethPeriod = teethPeriod;
            TeethShift = teethShift;
            LipsPeriod = lipsPeriod;
            LipsShift = lipsShift;
        }

        protected override void SetupReader()
        {
            Reader.AddMapping("Bars", b => b);
        }

        protected override void SetupWriter()
        {
            Writer.AddMapping("Jaws", AnswerBuffers[CurBufferIndex][0]);
            Writer.AddMapping("Teeth", AnswerBuffers[CurBufferIndex][1]);
            Writer.AddMapping("Lips", AnswerBuffers[CurBufferIndex][2]);
        }

        protected override void SetupBuilder()
        {
            Builder.Reset();
            Builder.SetParameter("JawsPeriod", JawsPeriod);
            Builder.SetParameter("JawsShift", JawsShift);
            Builder.SetParameter("TeethPeriod", TeethPeriod);
            Builder.SetParameter("TeethShift", TeethShift);
            Builder.SetParameter("LipsPeriod", LipsPeriod);
            Builder.SetParameter("LipsShift", LipsShift);
            Builder.SetParameter("TargetMethod", TargetMethod);
            Builder.SetParameter("TargetPrice", TargetPrice);
        }

        protected override List<double>[] CreateAnswerBuffer()
        {
            var res = new List<double>[3];
            for (var k = 0; k < 3; k++)
            {
                res[k] = new List<double>();
            }
            return res;
        }

        protected override void ReadAnswerUnit(BinaryReader reader, List<double>[] metaAnswer)
        {
            for (var k = 0; k < 3; k++)
            {
                metaAnswer[k].Add(reader.ReadDouble());
            }
        }

        protected override void CheckAnswerUnit(int index, List<double>[] metaAnswer)
        {
            for (var k = 0; k < 3; k++)
            {
                AnswerBuffers[CurBufferIndex][k][index] = double.IsNaN(AnswerBuffers[CurBufferIndex][k][index])
                    ? 0
                    : AnswerBuffers[CurBufferIndex][k][index];
                AssertX.Greater(Epsilon, Math.Abs(metaAnswer[k][index] - AnswerBuffers[CurBufferIndex][k][index]));
            }
        }
    }
}
