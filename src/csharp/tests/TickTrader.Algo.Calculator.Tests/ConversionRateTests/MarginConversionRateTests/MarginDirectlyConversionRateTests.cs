using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Calculator.Tests.ConversionRateTests
{
    //https://intranet.fxopen.org/wiki/pages/viewpage.action?pageId=24543580 formulas

    [TestClass]
    public sealed class MarginDirectlyConversionRateTests : ConversionManagerBase
    {
        [TestMethod]
        [DirectlyConvertionCategory]
        public void X_Equal_Z()
        {
            X = Z = "EUR";
            Y = "USD";

            _expected = () => 1.0;
            _actualFormula = _algoMarket.Conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y);
        }

        [TestMethod]
        [DirectlyConvertionCategory]
        public void Y_Equal_Z()
        {
            X = "EUR";
            Y = Z = "USD";

            _expected = () => Ask[X + Y];
            _actualFormula = _algoMarket.Conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y);
        }

        [TestMethod]
        [DirectlyConvertionCategory]
        public void XZ()
        {
            X = "EUR";
            Y = "USD";
            Z = "AUD";

            _expected = () => Ask[X + Z];
            _actualFormula = _algoMarket.Conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, X + Z);
        }

        [TestMethod]
        [DirectlyConvertionCategory]
        public void ZX()
        {
            X = "AUD";
            Y = "USD";
            Z = "EUR";

            _expected = () => 1.0 / Bid[Z + X];
            _actualFormula = _algoMarket.Conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, Z + X);
        }

        [TestMethod]
        [DirectlyConvertionCategory]
        public void YZ()
        {
            X = "EUR";
            Y = "AUD";
            Z = "CAD";

            _expected = () => Ask[X + Y] * Ask[Y + Z];
            _actualFormula = _algoMarket.Conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, Y + Z);
        }

        [TestMethod]
        [DirectlyConvertionCategory]
        public void ZY()
        {
            X = "EUR";
            Y = "USD";
            Z = "AUD";

            _expected = () => Ask[X + Y] / Bid[Z + Y];
            _actualFormula = _algoMarket.Conversion.GetMarginFormula;

            LoadSymbolsAndCheckConversionRate(X + Y, Z + Y);
        }
    }
}
