using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator.Tests.ConversionRateTests
{
    public abstract class ConversionManagerBase : AlgoMarketEmulator
    {
        protected Func<double> _expected;
        protected string X, Y, Z, C;

        internal delegate IConversionFormula BuildFormula(ISymbolInfo node);
        internal BuildFormula _actualFormula;


        protected Dictionary<string, double> Bid => _symbolStorage.Bid;

        protected Dictionary<string, double> Ask => _symbolStorage.Ask;


        public ConversionManagerBase()
        {
            _symbolStorage.AllSymbolsRateUpdate();
        }

        [TestInitialize]
        public void InitTestBaseState()
        {
            CreateAlgoMarket();

            X = Y = C = Z = null;
            _expected = null;
            _actualFormula = null;
        }

        protected void LoadSymbolsAndCheckConversionRate(params string[] load)
        {
            _account.BalanceCurrencyName = Z;

            InitAlgoMarket(load);

            var smbInfo = _algoMarket.Symbols.First(u => u.Name == X + Y);
            var actual = _actualFormula(smbInfo);

            Assert.AreEqual(actual.Value, _expected());
        }
    }
}
