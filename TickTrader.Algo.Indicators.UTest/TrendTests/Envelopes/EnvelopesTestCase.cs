using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.Envelopes
{
    public class EnvelopesTestCase : MethodsPricesTestCase<List<double>[]>
    {
        public int Period { get; protected set; }
        public int Shift { get; protected set; }
        public double Deviation { get; protected set; }

        public EnvelopesTestCase(Type indicatorType, string quotesPath, string answerPath, int period, int shift, double deviation)
            : base(indicatorType, quotesPath, answerPath, 4, 7, 16)
        {
            Period = period;
            Shift = shift;
            Deviation = deviation;
        }

        protected override void SetupReader()
        {
            Reader.AddMapping("Bars", b => b);
        }

        protected override void SetupWriter()
        {
            Writer.AddMapping("TopLine", AnswerBuffers[CurBufferIndex][0]);
            Writer.AddMapping("BottomLine", AnswerBuffers[CurBufferIndex][1]);
        }

        protected override void SetupBuilder()
        {
            Builder.Reset();
            Builder.SetParameter("Period", Period);
            Builder.SetParameter("Shift", Shift);
            Builder.SetParameter("TargetMethod", TargetMethod);
            Builder.SetParameter("TargetPrice", TargetPrice);
            Builder.SetParameter("Deviation", Deviation);
        }

        protected override List<double>[] CreateAnswerBuffer()
        {
            var res = new List<double>[2];
            for (var k = 0; k < 2; k++)
            {
                res[k] = new List<double>();
            }
            return res;
        }

        protected override void ReadAnswerUnit(BinaryReader reader, List<double>[] metaAnswer)
        {
            for (var k = 0; k < 2; k++)
            {
                metaAnswer[k].Add(reader.ReadDouble());
            }
        }

        protected override void CheckAnswerUnit(int index, List<double>[] metaAnswer)
        {
            for (var k = 0; k < 2; k++)
            {
                AnswerBuffers[CurBufferIndex][k][index] = double.IsNaN(AnswerBuffers[CurBufferIndex][k][index])
                    ? 0
                    : AnswerBuffers[CurBufferIndex][k][index];
                AssertX.Greater(Epsilon, Math.Abs(metaAnswer[k][index] - AnswerBuffers[CurBufferIndex][k][index]));
            }
        }
    }
}
