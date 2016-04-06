using System;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.StandardDeviation
{
    public class StandardDeviationTestCase : MethodsPricesTestCase
    {
        public StandardDeviationTestCase(Type indicatorType, string quotesPath, string answerPath, int period, int shift)
            : base(indicatorType, quotesPath, answerPath, 4, 7, 8, period, shift)
        {
        }

        protected override void SetupWriter()
        {
            Writer.AddMapping("StdDev", AnswerBuffers[CurBufferIndex][0]);
        }
    }
}
