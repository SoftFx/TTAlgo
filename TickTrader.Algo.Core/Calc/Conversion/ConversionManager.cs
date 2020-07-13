using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Common.Business;

namespace TickTrader.Algo.Core.Calc.Conversion
{
    public class ConversionManager
    {
        private MarketStateBase _market;
        private Dictionary<Tuple<string, string>, ISymbolInfo2> _convertionSet = new Dictionary<Tuple<string, string>, ISymbolInfo2>();
        private Dictionary<Tuple<string, string>, IConversionFormula> _marginConversions = new Dictionary<Tuple<string, string>, IConversionFormula>();
        private Dictionary<Tuple<string, string>, IConversionFormula> _posProfitConversions = new Dictionary<Tuple<string, string>, IConversionFormula>();
        private Dictionary<Tuple<string, string>, IConversionFormula> _negProfitConversions = new Dictionary<Tuple<string, string>, IConversionFormula>();

        public ConversionManager(MarketStateBase market)
        {
            _market = market;
        }

        internal void Init()
        {
            _convertionSet.Clear();
            _marginConversions.Clear();
            _posProfitConversions.Clear();
            _negProfitConversions.Clear();
            //.negAssetFormulas.Clear();
            FillConversionSet();
        }

        private void FillConversionSet()
        {
            foreach (var symbol in _market.Symbols)
            {
                var key = Tuple.Create(symbol.MarginCurrency, symbol.ProfitCurrency);
                if (!_convertionSet.ContainsKey(key))
                    _convertionSet.Add(key, symbol);
            }
        }

        internal IConversionFormula GetMarginFormula(SymbolMarketNode tracker, string toCurrency)
        {
            return _marginConversions.GetOrAdd(Tuple.Create(tracker.SymbolInfo.Name, toCurrency), () => BuildMarginFormula(tracker, toCurrency));
        }

        internal IConversionFormula GetPositiveProfitFormula(SymbolMarketNode tracker, string toCurrency)
        {
            return _posProfitConversions.GetOrAdd(Tuple.Create(tracker.SymbolInfo.Name, toCurrency), () => BuildPositiveProfitFormula(tracker, toCurrency));
        }

        internal IConversionFormula GetNegativeProfitFormula(SymbolMarketNode tracker, string toCurrency)
        {
            return _negProfitConversions.GetOrAdd(Tuple.Create(tracker.SymbolInfo.Name, toCurrency), () => BuildNegativeProfitFormula(tracker, toCurrency));
        }

        private IConversionFormula BuildMarginFormula(SymbolMarketNode tracker, string toCurrency)
        {
            ISymbolInfo2 XY = tracker.SymbolInfo;

            string X = XY.MarginCurrency;
            string Y = XY.ProfitCurrency;
            string Z = toCurrency;

            // N 1

            if (X == Z)
                return FormulaBuilder.Direct();

            // N 2

            if (Y == Z)
                return FormulaBuilder.Conversion(tracker, FxPriceType.Ask);

            // N 3

            ISymbolInfo2 XZ = GetFromSet(X, Z);

            if (XZ != null)
                return FormulaBuilder.Conversion(GetRate(XZ), FxPriceType.Ask);

            // N 4

            ISymbolInfo2 ZX = GetFromSet(Z, X);

            if (ZX != null)
                return FormulaBuilder.InverseConversion(GetRate(ZX), FxPriceType.Bid);

            // N 5

            ISymbolInfo2 YZ = GetFromSet(Y, Z);

            if (YZ != null)
                return FormulaBuilder.Conversion(GetRate(XY), FxPriceType.Ask)
                                     .Then(GetRate(YZ), FxPriceType.Ask);

            // N 6

            ISymbolInfo2 ZY = GetFromSet(Z, Y);

            if (ZY != null)
                return FormulaBuilder.Conversion(GetRate(XY), FxPriceType.Ask)
                                     .ThenDivide(GetRate(ZY), FxPriceType.Bid);

            foreach (var curr in _market.Currencies)
            {
                string C = curr.Name;

                // N 7

                ISymbolInfo2 XC = GetFromSet(X, C);
                ISymbolInfo2 ZC = GetFromSet(Z, C);

                if (XC != null && ZC != null)
                    return FormulaBuilder.Conversion(GetRate(XC), FxPriceType.Ask)
                                         .ThenDivide(GetRate(ZC), FxPriceType.Bid);

                // N 8

                ISymbolInfo2 CX = GetFromSet(C, X);

                if (CX != null && ZC != null)
                    return FormulaBuilder.InverseConversion(GetRate(CX), FxPriceType.Bid)
                                         .ThenDivide(GetRate(ZC), FxPriceType.Bid);

                // N 9

                ISymbolInfo2 CZ = GetFromSet(C, Z);

                if (XC != null && CZ != null)
                    return FormulaBuilder.Conversion(GetRate(XC), FxPriceType.Ask)
                                         .Then(GetRate(CZ), FxPriceType.Ask);

                // N 10

                if (CX != null && CZ != null)
                    return FormulaBuilder.InverseConversion(GetRate(CX), FxPriceType.Bid)
                                         .Then(GetRate(CZ), FxPriceType.Ask);

                // N 11

                ISymbolInfo2 YC = GetFromSet(Y, C);

                if (YC != null && ZC != null)
                    return FormulaBuilder.Conversion(GetRate(YC), FxPriceType.Ask)
                                         .ThenDivide(GetRate(ZC), FxPriceType.Bid)
                                         .Then(GetRate(XY), FxPriceType.Ask);

                // N 12

                ISymbolInfo2 CY = GetFromSet(C, Y);

                if (CY != null && ZC != null)
                    return FormulaBuilder.InverseConversion(GetRate(CY), FxPriceType.Bid)
                                         .ThenDivide(GetRate(ZC), FxPriceType.Bid)
                                         .Then(GetRate(XY), FxPriceType.Ask);

                // N 13

                if (YC != null && CZ != null)
                    return FormulaBuilder.Conversion(GetRate(YC), FxPriceType.Ask)
                                         .Then(GetRate(CZ), FxPriceType.Ask)
                                         .Then(GetRate(XY), FxPriceType.Ask);

                // N 14

                if (CY != null && CZ != null)
                    return FormulaBuilder.InverseConversion(GetRate(CY), FxPriceType.Bid)
                                         .Then(GetRate(CZ), FxPriceType.Ask)
                                         .Then(GetRate(XY), FxPriceType.Ask);
            }

            return FormulaBuilder.Error(XY, X, Z);
        }

        private IConversionFormula BuildPositiveProfitFormula(SymbolMarketNode tracker, string toCurrency)
        {
            return BuildProfitFormula(tracker, toCurrency, FxPriceType.Bid, FxPriceType.Ask);
        }

        private IConversionFormula BuildNegativeProfitFormula(SymbolMarketNode tracker, string toCurrency)
        {
            return BuildProfitFormula(tracker, toCurrency, FxPriceType.Ask, FxPriceType.Bid);
        }

        private IConversionFormula BuildProfitFormula(SymbolMarketNode tracker, string toCurrency, FxPriceType price1, FxPriceType price2)
        {
            ISymbolInfo2 XY = tracker.SymbolInfo;

            string X = XY.MarginCurrency;
            string Y = XY.ProfitCurrency;
            string Z = toCurrency;

            // N 1

            if (Y == Z)
                return FormulaBuilder.Direct();

            // N 2

            if (X == Z)
                return FormulaBuilder.InverseConversion(this.GetRate(XY), price2);

            // N 3

            ISymbolInfo2 YZ = GetFromSet(Y, Z);

            if (YZ != null)
                return FormulaBuilder.Conversion(GetRate(YZ), price1);

            // N 4

            ISymbolInfo2 ZY = GetFromSet(Z, Y);

            if (ZY != null)
                return FormulaBuilder.InverseConversion(GetRate(ZY), price2);

            // N 5

            ISymbolInfo2 ZX = GetFromSet(Z, X);

            if (ZX != null)
                return FormulaBuilder.InverseConversion(GetRate(XY), price2)
                                     .ThenDivide(GetRate(ZX), price2);

            // N 6

            ISymbolInfo2 XZ = GetFromSet(X, Z);

            if (XZ != null)
                return FormulaBuilder.InverseConversion(GetRate(XY), price2)
                                     .Then(GetRate(XZ), price1);

            foreach (var curr in this._market.Currencies)
            {
                string C = curr.Name;

                // N 7

                ISymbolInfo2 YC = GetFromSet(Y, C);
                ISymbolInfo2 ZC = GetFromSet(Z, C);

                if (YC != null && ZC != null)
                    return FormulaBuilder.Conversion(GetRate(YC), price1)
                                         .ThenDivide(GetRate(ZC), price2);

                // N 8

                ISymbolInfo2 CY = GetFromSet(C, Y);

                if (CY != null && ZC != null)
                    return FormulaBuilder.InverseConversion(GetRate(CY), price2)
                                         .ThenDivide(GetRate(ZC), price2);

                // N 9

                ISymbolInfo2 CZ = GetFromSet(C, Z);

                if (YC != null && CZ != null)
                    return FormulaBuilder.Conversion(GetRate(YC), price1)
                                         .Then(GetRate(CZ), price1);

                // N 10

                if (CY != null && CZ != null)
                    return FormulaBuilder.InverseConversion(GetRate(CY), price2)
                                         .Then(GetRate(CZ), price1);
            }

            return FormulaBuilder.Error(XY, Y, Z);
        }

        private ISymbolInfo2 GetFromSet(string currency1, string currency2)
        {
            return _convertionSet.GetOrDefault(Tuple.Create(currency1, currency2));
        }

        private SymbolMarketNode GetRate(ISymbolInfo2 symbol)
        {
            return _market.GetSymbolNodeInternal(symbol.Name) ?? throw new Exception("Unknown symbol: " + symbol.Name);
        }
    }
}
