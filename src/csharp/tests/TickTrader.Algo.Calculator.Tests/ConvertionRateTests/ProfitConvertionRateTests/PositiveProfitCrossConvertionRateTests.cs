using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TickTrader.Algo.Calculator.Tests.ConvertionRateTests.ProfitConvertionRateTests
{
    [TestClass]
    public sealed class PositiveProfitCrossConvertionRateTests : ProfitConvertionRateBase
    {
        protected override Dictionary<string, double> Price1 => Bid;

        protected override Dictionary<string, double> Price2 => Ask;


        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void YC_ZC()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_YC_ZC_test();
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void CY_ZC()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_CY_ZC_test();
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void YC_CZ()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_YC_CZ_test();
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void CY_CZ()
        {
            _actualFormula = _algoMarket.Conversion.GetPositiveProfitFormula;

            Run_CY_CZ_test();
        }
    }
}
