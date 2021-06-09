using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TestEnviroment;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator.Tests.ConvertionRateTests
{
    public abstract class ConvertionManagerBase
    {
        private static readonly SymbolInfoStorage _symbolStorage = SymbolInfoStorage.Instance;
        private static readonly CurrencyInfoStorage _currencyStorage = CurrencyInfoStorage.Instance;

        protected static AlgoMarketState _algoMarket;
        protected Func<double> _expected;

        protected string X, Y, Z, C;


        public delegate IConversionFormula BuildFormula(SymbolMarketNode node, string d);

        public BuildFormula _actualFormula;


        private static Dictionary<string, SymbolInfo> Symbols => _symbolStorage.Symbols;

        protected static Dictionary<string, double> Bid => _symbolStorage.Bid; // change to _algoMarket.Bids

        protected static Dictionary<string, double> Ask => _symbolStorage.Ask; // change to _algoMarket.Ask

        public ConvertionManagerBase()
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

        protected static AlgoMarketState InitAlgoMarket(string[] symbols)
        {
            _algoMarket.Init(symbols.Where(u => Symbols.ContainsKey(u)).Select(u => Symbols[u]), _currencyStorage.Currency?.Values);

            return _algoMarket;
        }

        protected void LoadSymbolsAndCheckConvertionRate(params string[] load)
        {
            var algoMarket = InitAlgoMarket(load);

            var smbNode = new SymbolMarketNode(algoMarket.Symbols.First(u => u.Name == X + Y));

            var actual = _actualFormula(smbNode, Z);

            Assert.AreEqual(actual.Value, _expected());
        }
    }
}
