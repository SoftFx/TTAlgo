using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OtherTests.HeikenAshi
{
    public class HeikenAshiTestCase : SimpleTestCase
    {
        public HeikenAshiTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 32)
        {
        }

        protected override void SetupParameters() { }

        protected override void GetOutput()
        {
            PutOutputToBuffer("HaLowHigh", 0);
            PutOutputToBuffer("HaHighLow", 1);
            PutOutputToBuffer("HaOpen", 2);
            PutOutputToBuffer("HaClose", 3);
        }

        protected override void CheckAnswer()
        {
            InvokeCheckAnswer($"{AnswerPath}.bin");
        }
    }
}
