using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestEnviroment;
using TickTrader.Algo.Calculator.AlgoMarket;

namespace TickTrader.Algo.Calculator.Tests.SideNodeTests
{
    [TestClass]
    public class AskSideNodeTests : LoadSymbolBase
    {
        private AskSideNode _askNode;

        [TestInitialize]
        public void IntiTest()
        {
            LoadSymbol();

            _askNode = new AskSideNode(_symbol);
        }

        [TestMethod]
        public void Empty_Node()
        {
            Assert.IsFalse(_askNode.HasValue);
            Assert.AreEqual(_askNode.Value, double.NaN);
        }

        [TestMethod]
        public void Check_Node_Value()
        {
            UpdateSymbolRate();

            Assert.IsTrue(_askNode.HasValue);
            Assert.AreEqual(_askNode.Value, _symbol.Ask);
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

            _askNode.Subscribe(_symbol);

            Check_Multiple_Update();
        }

        [TestMethod]
        public void Check_Unsubscribe_Method()
        {
            Check_Node_Value();

            _askNode.Subscribe(null);

            Empty_Node();
        }

        [TestMethod]
        public void Check_Unsubscribe_Subscribe()
        {
            Check_Unsubscribe_Method();

            LoadSymbol("AUDUSD");

            _askNode.Subscribe(_symbol);

            Check_Multiple_Update();
        }
    }
}
