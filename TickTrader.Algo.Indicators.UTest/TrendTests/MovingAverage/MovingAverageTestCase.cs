using System;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.MovingAverage
{
    public class MovingAverageTestCase : MethodsPricesTestCase
    {
        public double SmoothFactor { get; protected set; }

        public MovingAverageTestCase(Type indicatorType, string quotesPath, string answerPath, int period,
            int shift, double smoothFactor = 0.0) : base(indicatorType, quotesPath, answerPath, 4, 7, 8, period, shift)
        {
            SmoothFactor = smoothFactor;
        }

        protected override void SetupWriter()
        {
            Writer.AddMapping("MA", AnswerBuffers[CurBufferIndex][0]);
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("SmoothFactor", SmoothFactor);
        }
    }
}
