using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TickTrader.Algo.Calculator.Tests.ConversionRateTests
{
    [TestClass]
    public sealed class PositiveProfitCrossConversionRateTests : ProfitConversionRateBase
    {
        protected override Dictionary<string, double> Price1 => Bid;

        protected override Dictionary<string, double> Price2 => Ask;


        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void YC_ZC()
        {
            _actualFormula = _conversion.GetPositiveProfitFormula;

            Run_YC_ZC_test();
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void CY_ZC()
        {
            _actualFormula = _conversion.GetPositiveProfitFormula;

            Run_CY_ZC_test();
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void YC_CZ()
        {
            _actualFormula = _conversion.GetPositiveProfitFormula;

            Run_YC_CZ_test();
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void CY_CZ()
        {
            _actualFormula = _conversion.GetPositiveProfitFormula;

            Run_CY_CZ_test();
        }
    }
}
