using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.Alligator
{
    public class AlligatorTestCase : TestCase
    {
        protected List<double>[][] AnswerBuffers;
        protected int CurBufferIndex;

        public int JawsPeriod { get; protected set; }
        public int JawsShift { get; protected set; }
        public int TeethPeriod { get; protected set; }
        public int TeethShift { get; protected set; }
        public int LipsPeriod { get; protected set; }
        public int LipsShift { get; protected set; }
        public int TargetMethod { get; protected set; }
        public int TargetPrice { get; protected set; }

        public AlligatorTestCase(Type indicatorType, string quotesPath, string answerPath, int jawsPeriod, int jawsShift,
            int teethPeriod, int teethShift, int lipsPeriod, int lipsShift)
            : base(indicatorType, quotesPath, answerPath)
        {
            JawsPeriod = jawsPeriod;
            JawsShift = jawsShift;
            TeethPeriod = teethPeriod;
            TeethShift = teethShift;
            LipsPeriod = lipsPeriod;
            LipsShift = lipsShift;
            AnswerBuffers = new List<double>[4*7][];
            for (var i = 0; i < 4*7; i++)
            {
                AnswerBuffers[i] = new List<double>[3];
                for (var j = 0; j < 3; j++)
                {
                    AnswerBuffers[i][j] = new List<double>();
                }
            }
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

        protected override void CheckAnswer(string path)
        {
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 7; j++)
                {
                    TargetMethod = i;
                    TargetPrice = j;
                    CurBufferIndex = 7*i + j;
                    var metaAnswer = new List<double>[3];
                    for (var k = 0; k < 3; k++)
                    {
                        metaAnswer[k] = new List<double>();
                    }
                    var answerPath = $"{path}_{TargetMethod}_{TargetPrice}.bin";
                    using (var file = File.Open(answerPath, FileMode.Open, FileAccess.Read))
                    {
                        if (file.Length != Quotes.Count*24)
                        {
                            throw new ArgumentException("Meta answer is not equal to quotes count.");
                        }
                        using (var reader = new BinaryReader(file))
                        {
                            try
                            {
                                while (true)
                                {
                                    for (var k = 0; k < 3; k++)
                                    {
                                        metaAnswer[k].Add(reader.ReadDouble());
                                    }
                                }
                            }
                            catch (EndOfStreamException)
                            {
                            }
                        }
                    }
                    for (var q = 0; q < Quotes.Count; q++)
                        for (var k = 0; k < 3; k++)
                        {
                            AnswerBuffers[CurBufferIndex][k][q] = double.IsNaN(AnswerBuffers[CurBufferIndex][k][q])
                                ? 0
                                : AnswerBuffers[CurBufferIndex][k][q];
                            AssertX.Greater(Epsilon, Math.Abs(metaAnswer[k][q] - AnswerBuffers[CurBufferIndex][k][q]));
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
                for (var j = 0; j < 7; j++)
                {
                    TargetMethod = i;
                    TargetPrice = j;
                    CurBufferIndex = 7 * i + j;
                    Writer = new DirectWriter<Bar>();
                    SetupWriter();
                    Builder = new IndicatorBuilder<Bar>(IndicatorType, Reader, Writer);
                    SetupBuilder();
                    Builder.Build();
                }
        }
    }
}
