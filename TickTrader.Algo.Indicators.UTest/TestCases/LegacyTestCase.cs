using System;
using System.Collections.Generic;
using System.IO;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class LegacyTestCase<TAns> : SimpleTestCase<TAns>
    {
        protected LegacyTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
        }

        protected override void ReadQuotes()
        {
            Quotes = TTQuoteFileReader.ReadFile(QuotesPath);
        }

        protected abstract void ReadAnswerUnit(string str, TAns metaAnswer);

        protected override TAns ReadAnswer(string answerPath)
        {
            var metaAnswer = CreateAnswerBuffer();
            using (var reader = File.OpenText(answerPath))
            {
                string str;
                while ((str = reader.ReadLine())!=null)
                {
                    ReadAnswerUnit(str, metaAnswer);
                }
            }
            return metaAnswer;
        }
    }

    public abstract class LegacyTestCase : SimpleTestCase
    {
        protected LegacyTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int answerUnitSize) : base(indicatorType, symbol, quotesPath, answerPath, answerUnitSize)
        {
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
            using (var reader = File.OpenText(answerPath))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    ReadAnswerUnit(str, metaAnswer);
                }
            }
            if (metaAnswer.Length != Quotes.Count)
            {
                throw new ArgumentException("Meta answer is not equal to quotes count.");
            }
            return metaAnswer;
        }
    }
}
