using System;
using System.IO;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest
{
    public abstract class MethodsPricesTestCase<TAns> : TestCase
    {
        protected int MethodsCount;
        protected int PricesCount;
        protected int AnswerUnitSize;
        protected TAns[] AnswerBuffers; 
        protected int CurBufferIndex;

        public int TargetMethod { get; protected set; }
        public int TargetPrice { get; protected set; }

        protected abstract TAns CreateAnswerBuffer();

        protected MethodsPricesTestCase(Type indicatorType, string quotesPath, string answerPath, int methodsCount,
            int pricesCount, int answerUnitSize) : base(indicatorType, quotesPath, answerPath)
        {
            MethodsCount = methodsCount;
            PricesCount = pricesCount;
            AnswerUnitSize = answerUnitSize;
        }

        protected override void Setup()
        {
            AnswerBuffers = new TAns[MethodsCount*PricesCount];
            for (var i = 0; i < MethodsCount * PricesCount; i++)
            {
                AnswerBuffers[i] = CreateAnswerBuffer();
            }
            ReadQuotes();
            Reader = new DirectReader<Bar>(Quotes);
            SetupReader();
        }

        protected override void Run()
        {
            for (var i = 0; i < MethodsCount; i++)
                for (var j = 0; j < PricesCount; j++)
                {
                    TargetMethod = i;
                    TargetPrice = j;
                    CurBufferIndex = PricesCount*i + j;
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
            for (var i = 0; i < MethodsCount; i++)
                for (var j = 0; j < PricesCount; j++)
                {
                    TargetMethod = i;
                    TargetPrice = j;
                    CurBufferIndex = PricesCount*i + j;
                    var metaAnswer = CreateAnswerBuffer();
                    var answerPath = $"{path}_{TargetMethod}_{TargetPrice}.bin";
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
