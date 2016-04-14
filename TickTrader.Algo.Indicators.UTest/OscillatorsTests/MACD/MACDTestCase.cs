using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.MACD
{
    public class MacdTestCase : PricesTestCase
    {
        public int FastEma { get; protected set; }
        public int SlowEma { get; protected set; }
        public int MacdSma { get; protected set; }

        public MacdTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int fastEma,
            int slowEma, int macdSma) : base(indicatorType, symbol, quotesPath, answerPath, 16, 7)
        {
            FastEma = fastEma;
            SlowEma = slowEma;
            MacdSma = macdSma;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("FastEma", FastEma);
            SetBuilderParameter("SlowEma", SlowEma);
            SetBuilderParameter("MacdSma", MacdSma);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("MacdSeries", 0);
            PutOutputToBuffer("Signal", 1);
        }
    }
}
