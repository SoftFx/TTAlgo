using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.MovingAverage
{
    public class MovingAverageTestCase : MethodsPricesTestCase<List<double>>
    {
        public int Period { get; protected set; }
        public int Shift { get; protected set; }
        public double SmoothFactor { get; protected set; }

        public MovingAverageTestCase(Type indicatorType, string quotesPath, string answerPath, int period,
            int shift, double smoothFactor = 0.0) : base(indicatorType, quotesPath, answerPath, 4, 7, 8)
        {
            Period = period;
            Shift = shift;
            TargetMethod = 0;
            TargetPrice = 0;
            SmoothFactor = smoothFactor;
            CurBufferIndex = 0;
        }

        protected override void SetupReader()
        {
            Reader.AddMapping("Bars", b => b);
        }

        protected override void SetupWriter()
        {
            Writer.AddMapping("MA", AnswerBuffers[CurBufferIndex]);
        }

        protected override void SetupBuilder()
        {
            Builder.Reset();
            Builder.SetParameter("Period", Period);
            Builder.SetParameter("Shift", Shift);
            Builder.SetParameter("TargetMethod", TargetMethod);
            Builder.SetParameter("TargetPrice", TargetPrice);
            Builder.SetParameter("SmoothFactor", SmoothFactor);
        }

        protected override List<double> CreateAnswerBuffer()
        {
            return new List<double>();
        }

        protected override void ReadAnswerUnit(BinaryReader reader, List<double> metaAnswer)
        {
            metaAnswer.Add(reader.ReadDouble());
        }

        protected override void CheckAnswerUnit(int index, List<double> metaAnswer)
        {
            AnswerBuffers[CurBufferIndex][index] = double.IsNaN(AnswerBuffers[CurBufferIndex][index])
                            ? 0
                            : AnswerBuffers[CurBufferIndex][index];
            AssertX.Greater(Epsilon, Math.Abs(metaAnswer[index] - AnswerBuffers[CurBufferIndex][index]));
        }
    }
}
