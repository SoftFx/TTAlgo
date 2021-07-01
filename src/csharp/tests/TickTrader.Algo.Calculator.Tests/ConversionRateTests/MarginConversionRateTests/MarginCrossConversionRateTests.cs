using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Calculator.Tests.ConversionRateTests
{
    //https://intranet.fxopen.org/wiki/pages/viewpage.action?pageId=24543580 formulas

    [TestClass]
    public sealed class MarginCrossConversionRateTests : ConversionManagerBase
    {
        [TestMethod]
        [CrossMarginCurrencyCategory]
        public void XC_ZC()
        {
            X = "AUD";
            Y = "CAD";
            Z = "EUR";
            C = "USD";

            _expected = () => Ask[X + C] / Bid[Z + C];
            _actualFormula = _conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, X + C, Z + C);
        }

        [TestMethod]
        [CrossMarginCurrencyCategory]
        public void CX_ZC()
        {
            X = "USD";
            Y = "JPY";
            Z = "EUR";
            C = "AUD";

            _expected = () => 1.0 / Bid[C + X] / Bid[Z + C];
            _actualFormula = _conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, C + X, Z + C);
        }

        [TestMethod]
        [CrossMarginCurrencyCategory]
        public void XC_CZ()
        {
            X = "EUR";
            Y = "USD";
            Z = "CAD";
            C = "AUD";

            _expected = () => Ask[X + C] * Ask[C + Z];
            _actualFormula = _conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, X + C, C + Z);
        }

        [TestMethod]
        [CrossMarginCurrencyCategory]
        public void CX_CZ()
        {
            X = "AUD";
            Y = "CAD";
            Z = "USD";
            C = "EUR";

            _expected = () => 1.0 / Bid[C + X] * Ask[C + Z];
            _actualFormula = _conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, C + X, C + Z);
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void YC_ZC()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _expected = () => Ask[Y + C] / Bid[Z + C] * Ask[X + Y];
            _actualFormula = _conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, Y + C, Z + C);
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void CY_ZC()
        {
            X = "BTC";
            Y = "USD";
            Z = "EUR";
            C = "AUD";

            _expected = () => 1.0 / Bid[C + Y] / Bid[Z + C] * Ask[X + Y];
            _actualFormula = _conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, C + Y, Z + C);
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void YC_CZ()
        {
            X = "EUR";
            Y = "AUD";
            Z = "JPY";
            C = "USD";

            _expected = () => Ask[Y + C] * Ask[C + Z] * Ask[X + Y];
            _actualFormula = _conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, Y + C, C + Z);
        }

        [TestMethod]
        [CrossProfitCurrencyCategory]
        public void CY_CZ()
        {
            X = "EUR";
            Y = "USD";
            Z = "CAD";
            C = "AUD";

            _expected = () => 1.0 / Bid[C + Y] * Ask[C + Z] * Ask[X + Y];
            _actualFormula = _conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, C + Y, C + Z);
        }
    }
}
