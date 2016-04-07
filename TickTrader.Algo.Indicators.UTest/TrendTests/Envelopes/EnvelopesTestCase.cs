using System;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.Envelopes
{
    public class EnvelopesTestCase : MethodsPricesTestCase
    {
        public double Deviation { get; protected set; }

        public EnvelopesTestCase(Type indicatorType, string quotesPath, string answerPath, int period, int shift, double deviation)
            : base(indicatorType, quotesPath, answerPath, 4, 7, 16, period, shift)
        {
            Deviation = deviation;
        }

        protected override void SetupWriter()
        {
            Writer.AddMapping("TopLine", AnswerBuffers[CurBufferIndex][0]);
            Writer.AddMapping("BottomLine", AnswerBuffers[CurBufferIndex][1]);
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("Deviation", Deviation);
        }
    }
}
