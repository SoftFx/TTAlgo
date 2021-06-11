using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TickTrader.Algo.Calculator.Tests.ConversionRateTests
{
    [TestClass]
    public sealed class NegativeProfitCrossConversionRateTests : ProfitConversionRateBase
    {
        protected override Dictionary<string, double> Price1 => Ask;

        protected override Dictionary<string, double> Price2 => Bid;


        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void YC_ZC()
        {
            _actualFormula = _algoMarket.Conversion.GetNegativeProfitFormula;

            Run_YC_ZC_test();
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void CY_ZC()
        {
            _actualFormula = _algoMarket.Conversion.GetNegativeProfitFormula;

            Run_CY_ZC_test();
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void YC_CZ()
        {
            _actualFormula = _algoMarket.Conversion.GetNegativeProfitFormula;

            Run_YC_CZ_test();
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void CY_CZ()
        {
            _actualFormula = _algoMarket.Conversion.GetNegativeProfitFormula;

            Run_CY_CZ_test();
        }
    }
}
