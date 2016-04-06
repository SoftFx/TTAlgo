using System;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.BollingerBands
{
    public class BollingerBandsTestCase : PricesTestCase
    {
        public double Deviations { get; protected set; }

        public BollingerBandsTestCase(Type indicatorType, string quotesPath, string answerPath, int period, int shift,
            double deviations) : base(indicatorType, quotesPath, answerPath, 7, 24, period, shift)
        {
            Deviations = deviations;
            Epsilon = 23e-10;
        }


        protected override void SetupWriter()
        {
            Writer.AddMapping("MiddleLine", AnswerBuffers[CurBufferIndex][0]);
            Writer.AddMapping("TopLine", AnswerBuffers[CurBufferIndex][1]);
            Writer.AddMapping("BottomLine", AnswerBuffers[CurBufferIndex][2]);
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("Deviations", Deviations);
        }
    }
}
