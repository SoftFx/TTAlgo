using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestEnviroment;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator.Tests.AlgoMarketStateTests
{
    [TestClass]
    public sealed class SymbolMarketNodeTests : AlgoMarketEmulator
    {
        private ISymbolInfoWithRate _symbol;
        private SymbolMarketNode _node;

        [TestInitialize]
        public void TestInit()
        {
            CreateAlgoMarket();

            _account.BalanceCurrencyName = "EUR";

            InitAlgoMarket("EURUSD", "AUDCAD");

            _symbol = Symbol["EURUSD"];

            _node = new SymbolMarketNode(_account, _symbol.BuildNullQuote());
            _node.InitCalculators(_conversion);
        }

        [TestMethod]
        public void Empty_Node()
        {
            Assert.IsFalse(_node.IsShadowCopy);
            Assert.IsFalse(_node.Bid.HasValue);
            Assert.IsFalse(_node.Ask.HasValue);

            Assert.AreEqual(_node.Bid.Value, double.NaN);
            Assert.AreEqual(_node.Ask.Value, double.NaN);
        }

        [TestMethod]
        public void Update()
        {
            _symbol.BuildNewQuote();

            Assert.IsFalse(_node.IsShadowCopy);
            Assert.IsTrue(_node.Bid.HasValue);
            Assert.IsTrue(_node.Ask.HasValue);

            Assert.AreEqual(_node.Bid.Value, _symbol.Bid);
            Assert.AreEqual(_node.Ask.Value, _symbol.Ask);
        }

        [TestMethod]
        public void MultipleUpdate()
        {
            Empty_Node();

            Update();
            Update();
        }

        [TestMethod]
        public void Unsibscribe()
        {
            Empty_Node();
            Update();

            _node.Update(null);

            Assert.IsTrue(_node.IsShadowCopy);
            Assert.IsFalse(_node.Bid.HasValue);
            Assert.IsFalse(_node.Ask.HasValue);

            Assert.AreEqual(_node.Bid.Value, double.NaN);
            Assert.AreEqual(_node.Ask.Value, double.NaN);
        }

        [TestMethod]
        public void Resubscribe()
        {
            Empty_Node();
            Update();

            _symbol = Symbol["AUDCAD"];
            _node.Update(_symbol);

            Update();
        }

        [TestMethod]
        public void Unsibscribe_Subscribe()
        {
            Unsibscribe();

            _symbol = Symbol["AUDCAD"];
            _node.Update(_symbol);

            Update();
        }
    }
}
