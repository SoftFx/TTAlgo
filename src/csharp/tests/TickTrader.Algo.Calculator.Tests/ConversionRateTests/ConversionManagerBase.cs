using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TestEnviroment;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator.Tests.ConversionRateTests
{
    public abstract class ConversionManagerBase
    {
        private static readonly SymbolInfoStorage _symbolStorage = SymbolInfoStorage.Instance;
        private static readonly CurrencyInfoStorage _currencyStorage = CurrencyInfoStorage.Instance;

        protected static AlgoMarketState _algoMarket;
        protected Func<double> _expected;

        protected string X, Y, Z, C;


        internal delegate IConversionFormula BuildFormula(ISymbolInfo node, string d);

        internal BuildFormula _actualFormula;


        private static Dictionary<string, SymbolInfo> Symbols => _symbolStorage.Symbols;

        protected static Dictionary<string, double> Bid => _symbolStorage.Bid; // change to _algoMarket.Bids

        protected static Dictionary<string, double> Ask => _symbolStorage.Ask; // change to _algoMarket.Ask

        public ConversionManagerBase()
        {
            _symbolStorage.AllSymbolsRateUpdate();
        }

        [TestInitialize]
        public void InitTestBaseState()
        {
            _algoMarket = new AlgoMarketState();

            X = Y = C = Z = null;
            _expected = null;
            _actualFormula = null;
        }

        protected void LoadSymbolsAndCheckConversionRate(params string[] load)
        {
            _algoMarket.Init(load.Where(u => Symbols.ContainsKey(u)).Select(u => Symbols[u]), _currencyStorage.Currency?.Values);

            var smbInfo = _algoMarket.Symbols.First(u => u.Name == X + Y);

            var actual = _actualFormula(smbInfo, Z);

            Assert.AreEqual(actual.Value, _expected());
        }
    }
}
