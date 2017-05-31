using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.FATLSignal
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
            Builder.Symbols.Add(new Core.SymbolEntity(Symbol)
            {
                Digits = Digits,
                BaseCurrencyCode = string.Empty,
                CounterCurrencyCode = string.Empty
            }, new Dictionary<string, Core.CurrencyEntity>());
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Up", 0);
            PutOutputToBuffer("Down", 1);
        }
    }
}
