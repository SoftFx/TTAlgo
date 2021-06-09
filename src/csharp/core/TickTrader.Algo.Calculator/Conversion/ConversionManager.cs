using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    public enum PriceType
    {
        Bid = 0,
        Ask = 1,
    }

    internal class ConversionManager
    {
        private MarketStateBase _market;
        private Dictionary<Tuple<string, string>, ISymbolInfo> _convertionSet = new Dictionary<Tuple<string, string>, ISymbolInfo>();
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
                var key = Tuple.Create(symbol.BaseCurrency, symbol.CounterCurrency);
                if (!_convertionSet.ContainsKey(key))
                    _convertionSet.Add(key, symbol);
            }
        }

        internal IConversionFormula GetMarginFormula(SymbolMarketNode tracker, string depositCurr)
        {
            return _marginConversions.GetOrAdd(Tuple.Create(tracker.SymbolInfo.Name, depositCurr), () => BuildMarginFormula(tracker, depositCurr));
        }

        public IConversionFormula GetPositiveProfitFormula(SymbolMarketNode tracker, string depositCurr)
        {
            return _posProfitConversions.GetOrAdd(Tuple.Create(tracker.SymbolInfo.Name, depositCurr), () => BuildPositiveProfitFormula(tracker, depositCurr));
        }

        public IConversionFormula GetNegativeProfitFormula(SymbolMarketNode tracker, string depositCurr)
        {
            return _negProfitConversions.GetOrAdd(Tuple.Create(tracker.SymbolInfo.Name, depositCurr), () => BuildNegativeProfitFormula(tracker, depositCurr));
        }

        private IConversionFormula BuildMarginFormula(SymbolMarketNode tracker, string depositCurr)
        {
            ISymbolInfo XY = tracker.SymbolInfo;

            string X = XY.MarginCurrency;
            string Y = XY.ProfitCurrency;
            string Z = depositCurr;

            // N 1

            if (X == Z)
                return FormulaBuilder.Direct();

            // N 2

            if (Y == Z)
                return FormulaBuilder.Conversion(tracker, PriceType.Ask);

            // N 3

            ISymbolInfo XZ = GetFromSet(X, Z);

            if (XZ != null)
                return FormulaBuilder.Conversion(GetRate(XZ), PriceType.Ask);

            // N 4

            ISymbolInfo ZX = GetFromSet(Z, X);

            if (ZX != null)
                return FormulaBuilder.InverseConversion(GetRate(ZX), PriceType.Bid);

            // N 5

            ISymbolInfo YZ = GetFromSet(Y, Z);

            if (YZ != null)
                return FormulaBuilder.Conversion(GetRate(XY), PriceType.Ask)
                                     .Then(GetRate(YZ), PriceType.Ask);

            // N 6

            ISymbolInfo ZY = GetFromSet(Z, Y);

            if (ZY != null)
                return FormulaBuilder.Conversion(GetRate(XY), PriceType.Ask)
                                     .ThenDivide(GetRate(ZY), PriceType.Bid);

            foreach (var curr in _market.Currencies)
            {
                string C = curr.Name;

                // N 7

                ISymbolInfo XC = GetFromSet(X, C);
                ISymbolInfo ZC = GetFromSet(Z, C);

                if (XC != null && ZC != null)
                    return FormulaBuilder.Conversion(GetRate(XC), PriceType.Ask)
                                         .ThenDivide(GetRate(ZC), PriceType.Bid);

                // N 8

                ISymbolInfo CX = GetFromSet(C, X);

                if (CX != null && ZC != null)
                    return FormulaBuilder.InverseConversion(GetRate(CX), PriceType.Bid)
                                         .ThenDivide(GetRate(ZC), PriceType.Bid);

                // N 9

                ISymbolInfo CZ = GetFromSet(C, Z);

                if (XC != null && CZ != null)
                    return FormulaBuilder.Conversion(GetRate(XC), PriceType.Ask)
                                         .Then(GetRate(CZ), PriceType.Ask);

                // N 10

                if (CX != null && CZ != null)
                    return FormulaBuilder.InverseConversion(GetRate(CX), PriceType.Bid)
                                         .Then(GetRate(CZ), PriceType.Ask);

                // N 11

                ISymbolInfo YC = GetFromSet(Y, C);

                if (YC != null && ZC != null)
                    return FormulaBuilder.Conversion(GetRate(YC), PriceType.Ask)
                                         .ThenDivide(GetRate(ZC), PriceType.Bid)
                                         .Then(GetRate(XY), PriceType.Ask);

                // N 12

                ISymbolInfo CY = GetFromSet(C, Y);

                if (CY != null && ZC != null)
                    return FormulaBuilder.InverseConversion(GetRate(CY), PriceType.Bid)
                                         .ThenDivide(GetRate(ZC), PriceType.Bid)
                                         .Then(GetRate(XY), PriceType.Ask);

                // N 13

                if (YC != null && CZ != null)
                    return FormulaBuilder.Conversion(GetRate(YC), PriceType.Ask)
                                         .Then(GetRate(CZ), PriceType.Ask)
                                         .Then(GetRate(XY), PriceType.Ask);

                // N 14

                if (CY != null && CZ != null)
                    return FormulaBuilder.InverseConversion(GetRate(CY), PriceType.Bid)
                                         .Then(GetRate(CZ), PriceType.Ask)
                                         .Then(GetRate(XY), PriceType.Ask);
            }

            return FormulaBuilder.Error(XY, X, Z);
        }

        private IConversionFormula BuildPositiveProfitFormula(SymbolMarketNode tracker, string toCurrency)
        {
            return BuildProfitFormula(tracker, toCurrency, PriceType.Bid, PriceType.Ask);
        }

        private IConversionFormula BuildNegativeProfitFormula(SymbolMarketNode tracker, string toCurrency)
        {
            return BuildProfitFormula(tracker, toCurrency, PriceType.Ask, PriceType.Bid);
        }

        private IConversionFormula BuildProfitFormula(SymbolMarketNode tracker, string toCurrency, PriceType price1, PriceType price2)
        {
            ISymbolInfo XY = tracker.SymbolInfo;

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

            ISymbolInfo YZ = GetFromSet(Y, Z);

            if (YZ != null)
                return FormulaBuilder.Conversion(GetRate(YZ), price1);

            // N 4

            ISymbolInfo ZY = GetFromSet(Z, Y);

            if (ZY != null)
                return FormulaBuilder.InverseConversion(GetRate(ZY), price2);

            // N 5

            ISymbolInfo ZX = GetFromSet(Z, X);

            if (ZX != null)
                return FormulaBuilder.InverseConversion(GetRate(XY), price2)
                                     .ThenDivide(GetRate(ZX), price2);

            // N 6

            ISymbolInfo XZ = GetFromSet(X, Z);

            if (XZ != null)
                return FormulaBuilder.InverseConversion(GetRate(XY), price2)
                                     .Then(GetRate(XZ), price1);

            foreach (var curr in this._market.Currencies)
            {
                string C = curr.Name;

                // N 7

                ISymbolInfo YC = GetFromSet(Y, C);
                ISymbolInfo ZC = GetFromSet(Z, C);

                if (YC != null && ZC != null)
                    return FormulaBuilder.Conversion(GetRate(YC), price1)
                                         .ThenDivide(GetRate(ZC), price2);

                // N 8

                ISymbolInfo CY = GetFromSet(C, Y);

                if (CY != null && ZC != null)
                    return FormulaBuilder.InverseConversion(GetRate(CY), price2)
                                         .ThenDivide(GetRate(ZC), price2);

                // N 9

                ISymbolInfo CZ = GetFromSet(C, Z);

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

        private ISymbolInfo GetFromSet(string currency1, string currency2)
        {
            return _convertionSet.GetOrDefault(Tuple.Create(currency1, currency2));
        }

        private SymbolMarketNode GetRate(ISymbolInfo symbol)
        {
            return _market.GetSymbolNodeInternal(symbol.Name) ?? throw new Exception("Unknown symbol: " + symbol.Name);
        }
    }
}
