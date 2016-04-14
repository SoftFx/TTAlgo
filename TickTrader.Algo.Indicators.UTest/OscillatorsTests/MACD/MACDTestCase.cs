using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.MACD
{
    public class MacdTestCase : PricesTestCase
    {
        public int FastEma { get; protected set; }
        public int SlowEma { get; protected set; }
        public int MacdSma { get; protected set; }

        public MacdTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int fastEma,
            int slowEma, int macdSma) : base(indicatorType, symbol, quotesPath, answerPath, 16, 7, 0, 0)
        {
            FastEma = fastEma;
            SlowEma = slowEma;
            MacdSma = macdSma;
        }

        protected override void SetupBuilder()
        {
            Builder.MainSymbol = Symbol;
            Builder.SetParameter("TargetPrice", TargetPrice);
            Builder.SetParameter("FastEma", FastEma);
            Builder.SetParameter("SlowEma", SlowEma);
            Builder.SetParameter("MacdSma", MacdSma);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("MacdSeries"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("Signal"));
        }
    }
}
