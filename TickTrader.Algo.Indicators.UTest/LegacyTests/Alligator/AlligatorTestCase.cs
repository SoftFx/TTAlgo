using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Alligator
{
    public class AlligatorTestCase : LegacyTestCase
    {
        public int InpJawsPeriod { get; set; }
        public int InpJawsShift { get; set; }
        public int InpTeethPeriod { get; set; }
        public int InpTeethShift { get; set; }
        public int InpLipsPeriod { get; set; }
        public int InpLipsShift { get; set; }

        public AlligatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int inpJawsPeriod, int inpJawsShift, int inpTeethPeriod, int inpTeethShift, int inpLipsPeriod,
            int inpLipsShift) : base(indicatorType, symbol, quotesPath, answerPath, 24, 200)
        {
            InpJawsPeriod = inpJawsPeriod;
            InpJawsShift = inpJawsShift;
            InpTeethPeriod = inpTeethPeriod;
            InpTeethShift = inpTeethShift;
            InpLipsPeriod = inpLipsPeriod;
            InpLipsShift = inpLipsShift;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("InpJawsPeriod", InpJawsPeriod);
            Builder.SetParameter("InpJawsShift", InpJawsShift);
            Builder.SetParameter("InpTeethPeriod", InpTeethPeriod);
            Builder.SetParameter("InpTeethShift", InpTeethShift);
            Builder.SetParameter("InpLipsPeriod", InpLipsPeriod);
            Builder.SetParameter("InpLipsShift", InpLipsShift);
        }

        public override void InvokeFullBuildTest()
        {
            Epsilon = 53e-8;
            base.InvokeFullBuildTest();
        }

        public override void InvokeUpdateTest()
        {
            Epsilon = 1e-9;
            base.InvokeUpdateTest();
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtBlueBuffer"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("ExtRedBuffer"));
            AnswerBuffer[2] = new List<double>(Builder.GetOutput<double>("ExtLimeBuffer"));
        }
    }
}
