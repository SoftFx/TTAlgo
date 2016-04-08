using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

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

        public AlligatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int jawsPeriod,
            int jawsShift, int teethPeriod, int teethShift, int lipsPeriod, int lipsShift)
            : base(indicatorType, symbol, quotesPath, answerPath, 4, 7, 24, 0, 0)
        {
            JawsPeriod = jawsPeriod;
            JawsShift = jawsShift;
            TeethPeriod = teethPeriod;
            TeethShift = teethShift;
            LipsPeriod = lipsPeriod;
            LipsShift = lipsShift;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("JawsPeriod", JawsPeriod);
            Builder.SetParameter("JawsShift", JawsShift);
            Builder.SetParameter("TeethPeriod", TeethPeriod);
            Builder.SetParameter("TeethShift", TeethShift);
            Builder.SetParameter("LipsPeriod", LipsPeriod);
            Builder.SetParameter("LipsShift", LipsShift);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("Jaws"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("Teeth"));
            AnswerBuffer[2] = new List<double>(Builder.GetOutput<double>("Lips"));
        }
    }
}
