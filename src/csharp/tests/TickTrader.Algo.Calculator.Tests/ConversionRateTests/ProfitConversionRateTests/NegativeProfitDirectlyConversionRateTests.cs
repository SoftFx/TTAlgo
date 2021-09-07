using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TickTrader.Algo.Calculator.Tests.ConversionRateTests
{
    [TestClass]
    public sealed class NegativeProfitDirectlyConversionRateTests : ProfitConversionRateBase
    {
        protected override Dictionary<string, double> Price1 => Ask;

        protected override Dictionary<string, double> Price2 => Bid;


        [TestMethod, DirectlyConvertionCategory]
        public void X_Equal_Z()
        {
            _actualFormula = _conversion.GetNegativeProfitFormula;

            Run_X_Equal_Z_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void Y_Equal_Z()
        {
            _actualFormula = _conversion.GetNegativeProfitFormula;

            Run_Y_Equal_Z_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void XZ()
        {
            _actualFormula = _conversion.GetNegativeProfitFormula;

            Run_XZ_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void ZX()
        {
            _actualFormula = _conversion.GetNegativeProfitFormula;

            Run_ZX_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void YZ()
        {
            _actualFormula = _conversion.GetNegativeProfitFormula;

            Run_YZ_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void ZY()
        {
            _actualFormula = _conversion.GetNegativeProfitFormula;

            Run_ZY_test();
        }
    }
}
