using System;
using System.IO;
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
}
