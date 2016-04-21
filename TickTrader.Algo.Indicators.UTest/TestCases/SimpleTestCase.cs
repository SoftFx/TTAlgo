using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class SimpleTestCase<TAns> : TestCase
    {
        protected int AnswerUnitSize;
        protected TAns AnswerBuffer;

        protected abstract TAns CreateAnswerBuffer();

        protected SimpleTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize) : base(indicatorType, symbol, quotesPath, answerPath)
        {
            AnswerUnitSize = answerUnitSize;
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapBars(Builder, Symbol);
        }

        protected override void Setup()
        {
            base.Setup();
            AnswerBuffer = CreateAnswerBuffer();
        }

        protected virtual void InvokeLaunchTest(Action runAction)
        {
            runAction();
            CheckAnswer();
        }

        protected override void LaunchTest(Action runAction)
        {
            Setup();
            InvokeLaunchTest(runAction);
        }

        protected abstract void ReadAnswerUnit(BinaryReader reader, TAns metaAnswer);
        protected abstract void CheckAnswerUnit(int index, TAns metaAnswer);

        protected virtual TAns ReadAnswer(string answerPath)
        {
            var metaAnswer = CreateAnswerBuffer();
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
            return metaAnswer;
        }

        protected virtual void InvokeCheckAnswer(string answerPath)
        {
            var metaAnswer = ReadAnswer(answerPath);
            for (var k = 0; k < Quotes.Count; k++)
            {
                CheckAnswerUnit(k, metaAnswer);
            }
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer(AnswerPath);
        }
    }

    public abstract class SimpleTestCase : SimpleTestCase<List<double>[]>
    {
        protected SimpleTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
        }

        protected virtual void PutOutputToBuffer(string outputName, int bufferIndex)
        {
            AnswerBuffer[bufferIndex] = new List<double>(Builder.GetOutput<double>(outputName));
        }

        protected override List<double>[] CreateAnswerBuffer()
        {
            var res = new List<double>[AnswerUnitSize / 8];
            for (var k = 0; k < AnswerUnitSize / 8; k++)
            {
                res[k] = new List<double>();
            }
            return res;
        }

        protected override void ReadAnswerUnit(BinaryReader reader, List<double>[] metaAnswer)
        {
            for (var k = 0; k < AnswerUnitSize / 8; k++)
            {
                metaAnswer[k].Add(reader.ReadDouble());
            }
        }

        protected override void CheckAnswerUnit(int index, List<double>[] metaAnswer)
        {
            for (var k = 0; k < AnswerUnitSize / 8; k++)
            {
                AnswerBuffer[k][index] = double.IsNaN(AnswerBuffer[k][index])
                    ? 0
                    : AnswerBuffer[k][index];
                AssertX.Greater(Epsilon, Math.Abs(metaAnswer[k][index] - AnswerBuffer[k][index]));
            }
        }
    }
}
