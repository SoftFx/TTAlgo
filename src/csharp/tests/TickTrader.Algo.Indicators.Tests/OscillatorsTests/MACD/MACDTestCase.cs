using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.MACD
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

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("FastEma", FastEma);
            SetParameter("SlowEma", SlowEma);
            SetParameter("MacdSma", MacdSma);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("MacdSeries", 0);
            PutOutputToBuffer("Signal", 1);
        }
    }
}
