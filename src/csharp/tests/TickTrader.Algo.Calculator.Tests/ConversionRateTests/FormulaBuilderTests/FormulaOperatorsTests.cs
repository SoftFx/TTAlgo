using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestEnviroment;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Calculator.TradeSpecificsCalculators;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.Tests.FormulaBuilderTests
{
    [TestClass]
    public class FormulaOperatorsTests : LoadSymbolBase
    {
        private const double TestValue = 2.1317;

        private ISideNode _node;

        public FormulaOperatorsTests() : base()
        {
            LoadSymbol();
        }

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

            TestSuccessful(actual, expected);
        }

        [TestMethod]
        public void Inverse_With_Zero()
        {
            var inv = new InvOperator();

            TestSuccessfulWithZero(inv.Calculate(0.0));
        }

        [TestMethod]
        public void Mul()
        {
            var mul = new MulOperator(_node);

            var actual = mul.Calculate(TestValue);
            var expected = Mul(TestValue, _node.Value);

            TestSuccessful(actual, expected);
        }

        [TestMethod]
        public void Mul_By_Zero()
        {
            var mul = new MulOperator(_node);

            TestSuccessfulWithZero(mul.Calculate(0.0));
        }

        [TestMethod]
        public void Mul_With_Null_Base()
        {
            var mul = new MulOperator(null);

            TestFailedCrossSymbolNotFound(mul.Calculate(TestValue));
        }

        [TestMethod]
        public void Mul_With_Zero_Base()
        {
            _symbol.BuildZeroQuote();

            var mul = new MulOperator(_node);

            TestSuccessfulWithZero(mul.Calculate(TestValue));
        }

        [TestMethod]
        public void Mul_With_One_Side_Base()
        {
            AskSideRate();

            var mul = new MulOperator(_node);

            TestFailedOffCrossQuotes(mul.Calculate(TestValue));
        }

        [TestMethod]
        public void Div()
        {
            var div = new DivOperator(_node);

            var actual = div.Calculate(TestValue);
            var expected = Div(TestValue, _node.Value);

            TestSuccessful(actual, expected);
        }

        [TestMethod]
        public void Div_By_Zero()
        {
            var div = new DivOperator(_node);

            TestSuccessfulWithZero(div.Calculate(0.0));
        }

        [TestMethod]
        public void Div_With_Null_Base()
        {
            var div = new DivOperator(null);

            TestFailedCrossSymbolNotFound(div.Calculate(TestValue));
        }

        [TestMethod]
        public void Div_With_Zero_Base()
        {
            _symbol.BuildZeroQuote();

            var div = new DivOperator(_node);

            TestSuccessfulWithZero(div.Calculate(TestValue));
        }

        [TestMethod]
        public void Div_With_One_Side_Base()
        {
            AskSideRate();

            var div = new DivOperator(_node);

            TestFailedOffCrossQuotes(div.Calculate(TestValue));
        }


        protected static void TestSuccessful(CalculateResponse response, double expected)
        {
            Assert.IsTrue(response);
            Assert.AreEqual(CalculationError.None, response.Error);
            Assert.AreEqual(expected, response.Value);
        }

        protected static void TestSuccessfulWithZero(CalculateResponse response)
        {
            Assert.IsTrue(response);
            Assert.AreEqual(CalculationError.None, response.Error);
            Assert.AreEqual(0.0, response.Value);
        }

        protected static void TestFailedCrossSymbolNotFound(CalculateResponse response)
        {
            Assert.IsFalse(response);
            Assert.AreEqual(CalculationError.NoCrossSymbol, response.Error);
            Assert.AreEqual(0.0, response.Value);
        }

        protected static void TestFailedOffCrossQuotes(CalculateResponse response)
        {
            Assert.IsFalse(response);
            Assert.AreEqual(CalculationError.OffCrossQuote, response.Error);
            Assert.AreEqual(0.0, response.Value);
        }


        protected static double Inverse(double val) => 1.0 / val;

        protected static double Mul(double a, double b) => a * b;

        protected static double Div(double a, double b) => a / b;
    }
}
