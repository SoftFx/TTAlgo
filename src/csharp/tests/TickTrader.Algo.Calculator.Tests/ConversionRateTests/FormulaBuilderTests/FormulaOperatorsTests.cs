using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestEnviroment;
using TickTrader.Algo.Calculator.Operators;

namespace TickTrader.Algo.Calculator.Tests.FormulaBuilderTests
{
    [TestClass]
    public class FormulaOperatorsTests : LoadSymbolBase
    {
        private const double TestValue = 2.1317;

        private ISideNode _node;

        [ClassInitialize]
        public static void Init(TestContext _) => LoadSymbol();

        [TestInitialize]
        public void InitTestState()
        {
            UpdateSymbolRate();
            _node = new BidSideNode(_symbol);
        }

        [TestMethod]
        public void Inverse()
        {
            var inv = new InvOperator();

            var actual = inv.Calculate(TestValue);
            var expected = Inverse(TestValue);

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Inverse_With_Zero()
        {
            var inv = new InvOperator();

            WaitingForZero(inv.Calculate(0.0));
        }

        [TestMethod]
        public void Mul()
        {
            var mul = new MulOperator(_node);

            var actual = mul.Calculate(TestValue);
            var expected = Mul(TestValue, _node.Value);

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Mul_By_Zero()
        {
            var mul = new MulOperator(_node);

            WaitingForZero(mul.Calculate(0.0));
        }

        [TestMethod]
        public void Mul_With_Null_Base()
        {
            var mul = new MulOperator(null);

            WaitingForZero(mul.Calculate(TestValue));
        }

        [TestMethod]
        public void Mul_With_Zero_Base()
        {
            _symbol.BuildZeroQuote();

            var mul = new MulOperator(_node);

            WaitingForZero(mul.Calculate(TestValue));
        }

        [TestMethod]
        public void Mul_With_One_Side_Base()
        {
            AskSideRate();

            var mul = new MulOperator(_node);

            WaitingForZero(mul.Calculate(TestValue));
        }

        [TestMethod]
        public void Div()
        {
            var div = new DivOperator(_node);

            var actual = div.Calculate(TestValue);
            var expected = Div(TestValue, _node.Value);

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Div_By_Zero()
        {
            var div = new DivOperator(_node);

            WaitingForZero(div.Calculate(0.0));
        }

        [TestMethod]
        public void Div_With_Null_Base()
        {
            var div = new DivOperator(null);

            WaitingForZero(div.Calculate(TestValue));
        }

        [TestMethod]
        public void Div_With_Zero_Base()
        {
            ResetSymbolRate();

            var div = new DivOperator(_node);

            WaitingForZero(div.Calculate(TestValue));
        }

        [TestMethod]
        public void Div_With_One_Side_Base()
        {
            AskSideRate();

            var div = new DivOperator(_node);

            WaitingForZero(div.Calculate(TestValue));
        }

        protected static void WaitingForZero(double actual) => Assert.AreEqual(actual, 0.0);

        protected static double Inverse(double val) => 1.0 / val;

        protected static double Mul(double a, double b) => a * b;

        protected static double Div(double a, double b) => a / b;
    }
}
