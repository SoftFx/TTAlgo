using System;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.Alligator
{
    public class AlligatorTestCase : MethodsPricesTestCase
    {
        public int JawsPeriod { get; protected set; }
        public int JawsShift { get; protected set; }
        public int TeethPeriod { get; protected set; }
        public int TeethShift { get; protected set; }
        public int LipsPeriod { get; protected set; }
        public int LipsShift { get; protected set; }

        public AlligatorTestCase(Type indicatorType, string quotesPath, string answerPath, int jawsPeriod, int jawsShift,
            int teethPeriod, int teethShift, int lipsPeriod, int lipsShift)
            : base(indicatorType, quotesPath, answerPath, 4, 7, 24, 0, 0)
        {
            JawsPeriod = jawsPeriod;
            JawsShift = jawsShift;
            TeethPeriod = teethPeriod;
            TeethShift = teethShift;
            LipsPeriod = lipsPeriod;
            LipsShift = lipsShift;
        }

        protected override void SetupReader()
        {
            Reader.AddMapping("Bars", b => b);
        }

        protected override void SetupWriter()
        {
            Writer.AddMapping("Jaws", AnswerBuffers[CurBufferIndex][0]);
            Writer.AddMapping("Teeth", AnswerBuffers[CurBufferIndex][1]);
            Writer.AddMapping("Lips", AnswerBuffers[CurBufferIndex][2]);
        }

        protected override void SetupBuilder()
        {
            Builder.Reset();
            Builder.SetParameter("TargetMethod", TargetMethod);
            Builder.SetParameter("TargetPrice", TargetPrice);

            Builder.SetParameter("JawsPeriod", JawsPeriod);
            Builder.SetParameter("JawsShift", JawsShift);
            Builder.SetParameter("TeethPeriod", TeethPeriod);
            Builder.SetParameter("TeethShift", TeethShift);
            Builder.SetParameter("LipsPeriod", LipsPeriod);
            Builder.SetParameter("LipsShift", LipsShift);
        }
    }
}
