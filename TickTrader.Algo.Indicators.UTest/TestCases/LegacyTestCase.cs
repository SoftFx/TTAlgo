using System;
using System.Collections.Generic;
using System.IO;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class LegacyTestCase<TAns> : SimpleTestCase<TAns>
    {
        protected int AnswerShiftFront;
        protected int AnswerShiftEnd;

        protected LegacyTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int answerShiftFront = 0, int answerShiftEnd = 0)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            AnswerShiftFront = answerShiftFront;
            AnswerShiftEnd = answerShiftEnd;
        }

        protected override void ReadQuotes()
        {
            Quotes = TTQuoteFileReader.ReadFile(QuotesPath);
        }

        protected abstract void ReadAnswerUnit(string str, TAns metaAnswer);

        protected override TAns ReadAnswer(string answerPath)
        {
            var metaAnswer = CreateAnswerBuffer();
            var cnt = 0;
            using (var reader = File.OpenText(answerPath))
            {
                string str;
                while ((str = reader.ReadLine())!=null)
                {
                    ReadAnswerUnit(str, metaAnswer);
                    cnt++;
                }
            }
            if (cnt != Quotes.Count)
            {
                throw new ArgumentException("Meta answer is not equal to quotes count.");
            }
            return metaAnswer;
        }

        protected override void InvokeCheckAnswer(string answerPath)
        {
            var metaAnswer = ReadAnswer(answerPath);
            for (var k = AnswerShiftFront; k < Quotes.Count - AnswerShiftEnd; k++)
            {
                CheckAnswerUnit(k, metaAnswer);
            }
        }
    }

    public abstract class LegacyTestCase : SimpleTestCase
    {
        protected int AnswerShiftFront;
        protected int AnswerShiftEnd;

        protected LegacyTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize, int answerShiftFront = 0, int answerShiftEnd = 0)
            : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
            AnswerShiftFront = answerShiftFront;
            AnswerShiftEnd = answerShiftEnd;
        }

        protected override void ReadQuotes()
        {
            Quotes = TTQuoteFileReader.ReadFile(QuotesPath);
        }

        protected void ReadAnswerUnit(string str, List<double>[] metaAnswer)
        {
            if (str == null)
            {
                return;
            }
            var strSplit = str.Split('\t');
            for (var k = 0; k < AnswerUnitSize/8; k++)
            {
                metaAnswer[k].Add(
                    string.IsNullOrWhiteSpace(strSplit[k + 1])
                        ? 0
                        : Convert.ToDouble(strSplit[k + 1]));
            }
        }

        protected override List<double>[] ReadAnswer(string answerPath)
        {
            var metaAnswer = CreateAnswerBuffer();
            var cnt = 0;
            using (var reader = File.OpenText(answerPath))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    ReadAnswerUnit(str, metaAnswer);
                    cnt++;
                }
            }
            if (cnt != Quotes.Count)
            {
                throw new ArgumentException("Meta answer is not equal to quotes count.");
            }
            return metaAnswer;
        }

        protected override void InvokeCheckAnswer(string answerPath)
        {
            var metaAnswer = ReadAnswer(answerPath);
            for (var k = AnswerShiftFront; k < Quotes.Count - AnswerShiftEnd; k++)
            {
                CheckAnswerUnit(k, metaAnswer);
            }
        }
    }
}
