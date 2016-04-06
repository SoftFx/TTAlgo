using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest
{
    public abstract class PricesTestCase<TAns> : TestCase
    {
        protected int PricesCount;
        protected int AnswerUnitSize;
        protected TAns[] AnswerBuffers;
        protected int CurBufferIndex;

        public int TargetPrice { get; protected set; }

        protected abstract TAns CreateAnswerBuffer();

        protected PricesTestCase(Type indicatorType, string quotesPath, string answerPath, int pricesCount,
            int answerUnitSize) : base(indicatorType, quotesPath, answerPath)
        {
            PricesCount = pricesCount;
            AnswerUnitSize = answerUnitSize;
        }

        protected override void SetupReader()
        {
            Reader.AddMapping("Bars", b => b);
        }

        protected override void SetupBuilder()
        {
            Builder.Reset();
            Builder.SetParameter("TargetPrice", TargetPrice);
        }

        protected override void Setup()
        {
            AnswerBuffers = new TAns[PricesCount];
            for (var i = 0; i < PricesCount; i++)
            {
                AnswerBuffers[i] = CreateAnswerBuffer();
            }
            ReadQuotes();
            Reader = new DirectReader<Bar>(Quotes);
            SetupReader();
        }

        protected override void Run()
        {
            for (var i = 0; i < PricesCount; i++)
            {
                TargetPrice = i;
                CurBufferIndex = i;
                Writer = new DirectWriter<Bar>();
                SetupWriter();
                Builder = new IndicatorBuilder<Bar>(IndicatorType, Reader, Writer);
                SetupBuilder();
                Builder.Build();
            }
        }

        protected abstract void ReadAnswerUnit(BinaryReader reader, TAns metaAnswer);
        protected abstract void CheckAnswerUnit(int index, TAns metaAnswer);

        protected override void CheckAnswer(string path)
        {
                for (var i = 0; i < PricesCount; i++)
                {
                    TargetPrice = i;
                    CurBufferIndex = + i;
                    var metaAnswer = CreateAnswerBuffer();
                    var answerPath = $"{path}_{TargetPrice}.bin";
                    using (var file = File.Open(answerPath, FileMode.Open, FileAccess.Read))
                    {
                        if (file.Length != Quotes.Count*AnswerUnitSize)
                        {
                            throw new ArgumentException("Meta answer is not equal to quotes count.");
                        }
                        using (var reader = new BinaryReader(file))
                        {
                            try
                            {
                                while (true)
                                {
                                    ReadAnswerUnit(reader, metaAnswer);
                                }
                            }
                            catch (EndOfStreamException)
                            {
                            }
                        }
                    }
                    for (var k = 0; k < Quotes.Count; k++)
                    {
                        CheckAnswerUnit(k, metaAnswer);
                    }
                }
        }
    }

    public abstract class PricesTestCase : PricesTestCase<List<double>[]>
    {
        public int Period { get; protected set; }
        public int Shift { get; protected set; }

        protected PricesTestCase(Type indicatorType, string quotesPath, string answerPath, int pricesCount,
            int answerUnitSize, int period, int shift)
            : base(indicatorType, quotesPath, answerPath, pricesCount, answerUnitSize)
        {
            Period = period;
            Shift = shift;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("Period", Period);
            Builder.SetParameter("Shift", Shift);
        }

        protected override List<double>[] CreateAnswerBuffer()
        {
            var res = new List<double>[AnswerUnitSize/8];
            for (var k = 0; k < AnswerUnitSize/8; k++)
            {
                res[k] = new List<double>();
            }
            return res;
        }

        protected override void ReadAnswerUnit(BinaryReader reader, List<double>[] metaAnswer)
        {
            for (var k = 0; k < AnswerUnitSize/8; k++)
            {
                metaAnswer[k].Add(reader.ReadDouble());
            }
        }

        protected override void CheckAnswerUnit(int index, List<double>[] metaAnswer)
        {
            for (var k = 0; k < AnswerUnitSize/8; k++)
            {
                AnswerBuffers[CurBufferIndex][k][index] = double.IsNaN(AnswerBuffers[CurBufferIndex][k][index])
                    ? 0
                    : AnswerBuffers[CurBufferIndex][k][index];
                AssertX.Greater(Epsilon, Math.Abs(metaAnswer[k][index] - AnswerBuffers[CurBufferIndex][k][index]));
            }
        }
    }
}
