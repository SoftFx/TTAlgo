using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest
{
    public abstract class TestCase
    {
        public double Epsilon = 1e-9;

        protected List<Bar> Quotes;
        protected DirectReader<Bar> Reader;
        protected DirectWriter<Bar> Writer;
        protected IndicatorBuilder<Bar> Builder; 

        public Type IndicatorType { get; protected set; }
        public string QuotesPath { get; protected set; }
        public string AnswerPath { get; protected set; }

        protected TestCase(Type indicatorType, string quotesPath, string answerPath)
        {
            IndicatorType = indicatorType;
            QuotesPath = quotesPath;
            AnswerPath = answerPath;
        }

        protected virtual void ReadQuotes()
        {
            Quotes = TTQuoteBinaryFileReader.ReadQuotes(QuotesPath);
        }

        protected abstract void SetupReader();
        protected abstract void SetupWriter();
        protected abstract void SetupBuilder();
        protected abstract void CheckAnswer(string path);

        protected virtual void Setup()
        {
            ReadQuotes();
            Reader = new DirectReader<Bar>(Quotes);
            Writer = new DirectWriter<Bar>();
            SetupReader();
            SetupWriter();
            Builder = new IndicatorBuilder<Bar>(IndicatorType, Reader, Writer);
            SetupBuilder();
        }

        protected virtual void Run()
        {
            Builder.Build();
        }

        public void InvokeTest()
        {
            Setup();
            Run();
            CheckAnswer(AnswerPath);
        }
    }
}
