using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OtherTests.ZigZag
{
    public class ZigZagTestCase : SimpleTestCase
    {
        public int Depth { get; protected set; }
        public int Deviation { get; protected set; }
        public int Backstep { get; protected set; }
        public int Digits { get; protected set; }

        public ZigZagTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int depth,
            int deviation, int backstep, int digits) : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
            Depth = depth;
            Deviation = deviation;
            Backstep = backstep;
            Digits = digits;
        }

        protected override void SetupParameters()
        {
            SetParameter("Depth", Depth);
            SetParameter("Deviation", Deviation);
            SetParameter("Backstep", Backstep);
            Builder.Symbols.Add(new Core.SymbolEntity(Symbol)
            {
                Digits = Digits,
                BaseCurrencyCode = string.Empty,
                CounterCurrencyCode = string.Empty
            }, new Core.CurrenciesCollection());
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Zigzag", 0);
        }
    }
}
