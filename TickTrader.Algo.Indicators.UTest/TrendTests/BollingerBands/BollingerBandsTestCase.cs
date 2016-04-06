using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.BollingerBands
{
    public class BollingerBandsTestCase : PricesTestCase<List<double>[]>
    {
        public int Period { get; protected set; }
        public int Shift { get; protected set; }
        public double Deviations { get; protected set; }

        public BollingerBandsTestCase(Type indicatorType, string quotesPath, string answerPath, int period, int shift,
            double deviations) : base(indicatorType, quotesPath, answerPath, 7, 24)
        {
            Period = period;
            Shift = shift;
            Deviations = deviations;
            Epsilon = 23e-10;
        }

        protected override void SetupReader()
        {
            Reader.AddMapping("Bars", b => b);
        }

        protected override void SetupWriter()
        {
            Writer.AddMapping("MiddleLine", AnswerBuffers[CurBufferIndex][0]);
            Writer.AddMapping("TopLine", AnswerBuffers[CurBufferIndex][1]);
            Writer.AddMapping("BottomLine", AnswerBuffers[CurBufferIndex][2]);
        }

        protected override void SetupBuilder()
        {
            Builder.Reset();
            Builder.SetParameter("Period", Period);
            Builder.SetParameter("Shift", Shift);
            Builder.SetParameter("TargetPrice", TargetPrice);
            Builder.SetParameter("Deviations", Deviations);
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
