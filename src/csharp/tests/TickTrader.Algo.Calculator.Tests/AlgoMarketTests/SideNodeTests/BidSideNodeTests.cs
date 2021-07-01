using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestEnviroment;
using TickTrader.Algo.Calculator.AlgoMarket;

namespace TickTrader.Algo.Calculator.Tests.SideNodeTests
{
    [TestClass]
    public class BidSideNodeTests : LoadSymbolBase
    {
        private BidSideNode _bidNode;

        [TestInitialize]
        public void InitTest()
        {
            LoadSymbol();

            _bidNode = new BidSideNode(_symbol);
        }

        [TestMethod]
        public void Empty_Node()
        {
            Assert.IsFalse(_bidNode.HasValue);
            Assert.AreEqual(_bidNode.Value, double.NaN);
        }

        [TestMethod]
        public void Check_Node_Value()
        {
            UpdateSymbolRate();

            Assert.IsTrue(_bidNode.HasValue);
            Assert.AreEqual(_bidNode.Value, _symbol.Bid);
        }

        [TestMethod]
        public void Check_Multiple_Update()
        {
            Empty_Node();

            Check_Node_Value();
            Check_Node_Value();
        }

        [TestMethod]
        public void Check_Multiple_Update_With_Reset()
        {
            Check_Multiple_Update();

            ResetSymbolRate();

            Empty_Node();
        }

        [TestMethod]
        public void Check_Subscribe_Method()
        {
            Check_Node_Value();

            LoadSymbol("AUDUSD");

            _bidNode.Subscribe(_symbol);

            Check_Multiple_Update();
        }

        [TestMethod]
        public void Check_Unsubscribe_Method()
        {
            Check_Node_Value();

            _bidNode.Subscribe(null);

            Empty_Node();
        }

        [TestMethod]
        public void Check_Unsubscribe_Subscribe()
        {
            Check_Unsubscribe_Method();

            LoadSymbol("AUDUSD");

            _bidNode.Subscribe(_symbol);

            Check_Multiple_Update();
        }
    }
}
