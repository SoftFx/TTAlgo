using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    public static class SlippageConverters
    {
        public static double SlippagePipsToFractions(double slippage, double price, Symbol symbol)
        {
            switch (symbol.SlippageType)
            {
                case SlippageType.Percent:
                    return slippage;
                case SlippageType.Pips:
                    return SlippagePipsToPercent(slippage, price, symbol) / 100;
                default:
                    throw new ArgumentException($"Unsupported slippage type {symbol.SlippageType}");
            }
        }

        public static double SlippagePipsToPercent(double slippage, double price, Symbol symbol)
        {
            switch (symbol.SlippageType)
            {
                case SlippageType.Percent:
                    return slippage;
                case SlippageType.Pips:
                    return slippage * symbol.Point * 100 / price;
                default:
                    throw new ArgumentException($"Unsupported slippage type {symbol.SlippageType}");
            }
        }
    }
}
