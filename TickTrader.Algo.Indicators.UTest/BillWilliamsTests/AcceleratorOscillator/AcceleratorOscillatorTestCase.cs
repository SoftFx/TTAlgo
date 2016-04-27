using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.AcceleratorOscillator
{
    public class AcceleratorOscillatorTestCase : SimpleTestCase
    {
        public int FastSmaPeriod { get; protected set; }
        public int SlowSmaPeriod { get; protected set; }
        public int DataLimit { get; protected set; }

        public AcceleratorOscillatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int fastSmaPeriod, int slowSmaPeriod, int dataLimit)
            : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
            FastSmaPeriod = fastSmaPeriod;
            SlowSmaPeriod = slowSmaPeriod;
            DataLimit = dataLimit;
        }

        protected override void SetupParameters()
        {
            SetParameter("FastSmaPeriod", FastSmaPeriod);
            SetParameter("SlowSmaPeriod", SlowSmaPeriod);
            SetParameter("DataLimit", DataLimit);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ValueUp", 0);
            PutOutputToBuffer("ValueDown", 1);
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}.bin");
        }
    }
}
