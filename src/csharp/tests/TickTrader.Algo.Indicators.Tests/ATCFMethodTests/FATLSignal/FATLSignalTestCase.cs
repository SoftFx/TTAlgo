﻿using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.ATCFMethodTests.FATLSignal
{
    public class FatlSignalTestCase : DigitalIndicatorTestCase
    {
        public int Digits { get; protected set; }

        public FatlSignalTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int digits) : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
            Digits = digits;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            Builder.Symbols.Add(new Domain.SymbolInfo
            {
                Name = Symbol,
                Digits = Digits,
                BaseCurrency = string.Empty,
                CounterCurrency = string.Empty
            });
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Up", 0);
            PutOutputToBuffer("Down", 1);
        }
    }
}
