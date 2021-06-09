using System.Collections.Generic;

namespace TickTrader.Algo.Calculator.Tests.ConvertionRateTests
{
    public abstract class ProfitConvertionRateBase : ConvertionManagerBase
    {
        protected abstract Dictionary<string, double> Price1 { get; }

        protected abstract Dictionary<string, double> Price2 { get; }


        protected void Run_X_Equal_Z_test()
        {
            X = Z = "EUR";
            Y = "USD";

            _expected = () => 1.0 / Price2[X + Y];

            LoadSymbolsAndCheckConvertionRate(X + Y);
        }

        public void Run_Y_Equal_Z_test()
        {
            X = "EUR";
            Y = Z = "USD";

            _expected = () => 1.0;

            LoadSymbolsAndCheckConvertionRate(X + Y);
        }

        public void Run_XZ_test()
        {
            X = "EUR";
            Y = "USD";
            Z = "AUD";

            _expected = () => 1.0 / Price2[X + Y] * Price1[X + Z];

            LoadSymbolsAndCheckConvertionRate(X + Y, X + Z);
        }

        public void Run_ZX_test()
        {
            X = "AUD";
            Y = "USD";
            Z = "EUR";

            _expected = () => 1.0 / Price2[X + Y] / Price2[Z + X];

            LoadSymbolsAndCheckConvertionRate(X + Y, Z + X);
        }

        public void Run_YZ_test()
        {
            X = "EUR";
            Y = "AUD";
            Z = "CAD";

            _expected = () => Price1[Y + Z];

            LoadSymbolsAndCheckConvertionRate(X + Y, Y + Z);
        }

        public void Run_ZY_test()
        {
            X = "EUR";
            Y = "USD";
            Z = "AUD";

            _expected = () => 1 / Price2[Z + Y];

            LoadSymbolsAndCheckConvertionRate(X + Y, Z + Y);
        }

        public void Run_YC_ZC_test()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _expected = () => Price1[Y + C] / Price2[Z + C];

            LoadSymbolsAndCheckConvertionRate(X + Y, Y + C, Z + C);
        }

        public void Run_CY_ZC_test()
        {
            X = "BTC";
            Y = "USD";
            Z = "EUR";
            C = "AUD";

            _expected = () => 1.0 / Price2[C + Y] / Price2[Z + C];

            LoadSymbolsAndCheckConvertionRate(X + Y, C + Y, Z + C);
        }

        public void Run_YC_CZ_test()
        {
            X = "EUR";
            Y = "AUD";
            Z = "JPY";
            C = "USD";

            _expected = () => Price1[Y + C] * Price1[C + Z];

            LoadSymbolsAndCheckConvertionRate(X + Y, Y + C, C + Z);
        }

        public void Run_CY_CZ_test()
        {
            X = "EUR";
            Y = "USD";
            Z = "CAD";
            C = "AUD";

            _expected = () => 1.0 / Price2[C + Y] * Price1[C + Z];

            LoadSymbolsAndCheckConvertionRate(X + Y, C + Y, C + Z);
        }
    }
}
