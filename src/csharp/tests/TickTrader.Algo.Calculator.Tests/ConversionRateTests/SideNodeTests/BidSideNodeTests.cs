using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestEnviroment;

namespace TickTrader.Algo.Calculator.Tests.SideNodeTests
{
    [TestClass]
    public class BidSideNodeTests : LoadSymbolBase
    {
        private BidSideNode _bidNode;

        [ClassInitialize]
        public static void Init(TestContext _) => LoadSymbol();

        [TestInitialize]
        public void InitTest()
        {
            ResetSymbolRate();

            _bidNode = new BidSideNode(_symbol);
        }

        [TestMethod]
        public void Empty_Node()
        {
            Assert.IsFalse(_bidNode.HasValue);
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
    }
}
