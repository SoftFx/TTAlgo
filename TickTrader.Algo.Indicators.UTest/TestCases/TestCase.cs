using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.TestCases
{
    public abstract class TestCase
    {
        public double Epsilon = 1e-9;

        protected List<BarEntity> Quotes;
        protected IndicatorBuilder Builder; 

        public Type IndicatorType { get; protected set; }
        public string Symbol { get; protected set; }
        public string QuotesPath { get; protected set; }
        public string AnswerPath { get; protected set; }

        protected TestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
        {
            IndicatorType = indicatorType;
            Symbol = symbol;
            QuotesPath = quotesPath;
            AnswerPath = answerPath;
        }

        protected virtual void ReadQuotes()
        {
            Quotes = TTQuoteBinaryFileReader.ReadQuotes(QuotesPath);
        }

        protected virtual void SetupBuilder()
        {
            Builder.MainSymbol = Symbol;
        }

        protected abstract void SetupInput();
        protected abstract void GetOutput();
        protected abstract void CheckAnswer();

        protected virtual void Setup()
        {
            ReadQuotes();
            Builder = new IndicatorBuilder(AlgoPluginDescriptor.Get(IndicatorType));
            SetupBuilder();
            SetupInput();
        }

        protected virtual void RunFullBuild()
        {
            Builder.GetBarSeries(Symbol).Append(Quotes);
            Builder.BuildNext(Quotes.Count);
            GetOutput();
        }

        protected virtual void RunUpdate()
        {
            var firstQuote = Quotes[0];
            var inputBuffer = Builder.GetBarSeries(Symbol);
            foreach (var quote in Quotes)
            {
                inputBuffer.Append(firstQuote);
                Builder.BuildNext();
                inputBuffer.Last = quote;
                Builder.RebuildLast();
            }
            GetOutput();
        }

        protected virtual void LaunchTest(Action runAction)
        {
            Setup();
            runAction();
            CheckAnswer();
        }

        public virtual void InvokeFullBuildTest()
        {
            LaunchTest(RunFullBuild);
        }

        public virtual void InvokeUpdateTest()
        {
            LaunchTest(RunUpdate);
        }
    }
}
