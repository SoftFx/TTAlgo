using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.MovingAverage
{
    public class MovingAverageTestCase : TestCase
    {
        protected List<double>[] AnswerBuffers;
        protected int CurBufferIndex;

        public int Period { get; protected set; }
        public int Shift { get; protected set; }
        public int TargetMethod { get; protected set; }
        public int TargetPrice { get; protected set; }
        public double SmoothFactor { get; protected set; }

        public MovingAverageTestCase(Type indicatorType, string quotesPath, string answerPath, int period,
            int shift, double smoothFactor = 0.0) : base(indicatorType, quotesPath, answerPath)
        {
            Period = period;
            Shift = shift;
            TargetMethod = 0;
            TargetPrice = 0;
            SmoothFactor = smoothFactor;
            CurBufferIndex = 0;
            AnswerBuffers = new List<double>[4*6];
            for (var i = 0; i < 4*6; i++)
            {
                AnswerBuffers[i] = new List<double>();
            }
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

        protected override void CheckAnswer(string path)
        {
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 6; j++)
                {
                    TargetMethod = i;
                    TargetPrice = j;
                    CurBufferIndex = 6*i + j;
                    var metaAnswer = new List<double>();
                    var answerPath = $"{path}_{TargetMethod}_{TargetPrice}.bin";
                    using (var file = File.Open(answerPath, FileMode.Open, FileAccess.Read))
                    {
                        if (file.Length != Quotes.Count*8)
                        {
                            throw new ArgumentException("Meta answer is not equal to quotes count.");
                        }
                        using (var reader = new BinaryReader(file))
                        {
                            try
                            {
                                while (true)
                                {
                                    metaAnswer.Add(reader.ReadDouble());
                                }
                            }
                            catch (EndOfStreamException)
                            {
                            }
                        }
                    }
                    for (var k = 0; k < Quotes.Count; k++)
                    {
                        AnswerBuffers[CurBufferIndex][k] = double.IsNaN(AnswerBuffers[CurBufferIndex][k])
                            ? 0
                            : AnswerBuffers[CurBufferIndex][k];
                        AssertX.Greater(Epsilon, Math.Abs(metaAnswer[k] - AnswerBuffers[CurBufferIndex][k]));
                    }
                }
        }

        protected override void Setup()
        {
            ReadQuotes();
            Reader = new DirectReader<Bar>(Quotes);
            SetupReader();
        }

        protected override void Run()
        {
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 6; j++)
                {
                    TargetMethod = i;
                    TargetPrice = j;
                    CurBufferIndex = 6*i + j;
                    Writer = new DirectWriter<Bar>();
                    SetupWriter();
                    Builder = new IndicatorBuilder<Bar>(IndicatorType, Reader, Writer);
                    SetupBuilder();
                    Builder.Build();
                }
        }
    }
}
