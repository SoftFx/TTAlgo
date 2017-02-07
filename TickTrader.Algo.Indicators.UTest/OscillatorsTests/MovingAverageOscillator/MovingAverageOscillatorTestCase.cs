using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.MovingAverageOscillator
{
    public class MovingAverageOscillatorTestCase : PricesTestCase
    {
        public int FastEma { get; protected set; }
        public int SlowEma { get; protected set; }
        public int MacdSma { get; protected set; }

        public MovingAverageOscillatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int fastEma, int slowEma, int macdSma) : base(indicatorType, symbol, quotesPath, answerPath, 8, 7)
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
            PutOutputToBuffer("OsMa", 0);
        }
    }
}
