using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.AcceleratorOscillator
{
    public class AcceleratorOscillatorTestCase : SimpleTestCase
    {
        public AcceleratorOscillatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
        }

        protected override void SetupParameters() { }

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
