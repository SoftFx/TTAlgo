using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestEnviroment;

namespace TickTrader.Algo.Calculator.Tests.SideNodeTests
{
    [TestClass]
    public class AskSideNodeTests : LoadSymbolBase
    {
        private AskSideNode _askNode;

        [ClassInitialize]
        public static void Init(TestContext _) => LoadSymbol();

        [TestInitialize]
        public void IntiTest()
        {
            ResetSymbolRate();

            _askNode = new AskSideNode(_symbol);
        }

        [TestMethod]
        public void Empty_Node()
        {
            Assert.IsFalse(_askNode.HasValue);
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
    }
}
