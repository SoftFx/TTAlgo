﻿using System;
using System.Collections.Generic;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.TestCases
{
    public abstract class TestCase
    {
        public double Epsilon = 1e-9;

        protected List<BarData> Quotes;
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

        protected virtual void SetParameter(string paramName, object value)
        {
            Builder.SetParameter(paramName, value);
        }

        protected abstract void SetupInput();
        protected abstract void SetupParameters();
        protected abstract void GetOutput();
        protected abstract void CheckAnswer();

        protected virtual void Setup()
        {
            ReadQuotes();
            Builder = new IndicatorBuilder(new CoreV1.Metadata.PluginMetadata(IndicatorType))
            {
                MainSymbol = Symbol
            };
            SetupParameters();
            SetupInput();
        }

        protected virtual void RunFullBuild()
        {
            Builder.GetBarBuffer(Symbol).AppendRange(Quotes);
            Builder.BuildNext(Quotes.Count);
            GetOutput();
        }

        protected virtual void RunUpdate()
        {
            var prevQuote = Quotes[0];
            var inputBuffer = Builder.GetBarBuffer(Symbol);
            foreach (var quote in Quotes)
            {
                inputBuffer.Append(prevQuote);
                Builder.BuildNext();
                inputBuffer.Last = quote;
                Builder.RebuildLast();
                prevQuote = quote;
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
