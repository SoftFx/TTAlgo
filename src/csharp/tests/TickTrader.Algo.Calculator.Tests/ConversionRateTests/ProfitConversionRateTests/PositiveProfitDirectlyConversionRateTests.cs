using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TickTrader.Algo.Calculator.Tests.ConversionRateTests
{
    [TestClass]
    public sealed class PositiveProfitDirectlyConversionRateTests : ProfitConversionRateBase
    {
        protected override Dictionary<string, double> Price1 => Bid;

        protected override Dictionary<string, double> Price2 => Ask;


        [TestMethod, DirectlyConvertionCategory]
        public void X_Equal_Z()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_X_Equal_Z_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void Y_Equal_Z()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_Y_Equal_Z_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void XZ()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_XZ_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void ZX()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_ZX_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void YZ()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_YZ_test();
        }

        [TestMethod, DirectlyConvertionCategory]
        public void ZY()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_ZY_test();
        }
    }
}
