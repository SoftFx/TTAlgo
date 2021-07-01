using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestEnviroment;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.Tests.FormulaBuilderTests
{
    [TestClass]
    public class FormulaBuilderTests
    {
        private static SymbolInfoStorage _storage;
        private static SymbolInfo a, b, c;

        private IConversionFormula _formula;
        private ISideNode _nodeBidA, _nodeBidB, _nodeAskC;


        [ClassInitialize]
        public static void InitClass(TestContext _)
        {
            _storage = new SymbolInfoStorage();

            a = _storage.Symbols["EURUSD"];
            b = _storage.Symbols["EURAUD"];
            c = _storage.Symbols["AUDUSD"];
        }

        [TestInitialize]
        public void LoadSymbols()
        {
            _nodeBidA = new BidSideNode(a.BuildNewQuote());
            _nodeBidB = new BidSideNode(b.BuildNewQuote());
            _nodeAskC = new AskSideNode(c.BuildNewQuote());
        }

        internal FormulaBuilder Build_Inv_Mul_Div_Formula() => Formula.Inv(_nodeBidA).Mul(_nodeBidB).Div(_nodeAskC);

        internal double Actual_Inv_Mul_Div_Value => 1.0 / _nodeBidA.Value * _nodeBidB.Value / _nodeAskC.Value;


        [TestMethod]
        public void Direct_Call()
        {
            _formula = Formula.Direct;

            TestSuccessful(1.0);
        }

        [TestMethod]
        public void Direct_Singleton()
        {
            var direct1 = Formula.Direct;
            var direct2 = Formula.Direct;

            Assert.AreEqual(direct1, direct2);
        }

        [TestMethod]
        public void ErrorBuild_Call()
        {
            _formula = Formula.ErrorBuild;

            TestFailedCrossSymbolNotFound();
        }

        [TestMethod]
        public void ErrorBuild_Singleton()
        {
            var error1 = Formula.ErrorBuild;
            var error2 = Formula.ErrorBuild;

            Assert.AreEqual(error1, error2);
        }

        [TestMethod]
        public void Build_Formula()
        {
            _formula = Formula.Get(_nodeBidA);

            var actual = _nodeBidA.Value;

            TestSuccessful(actual);
        }

        [TestMethod]
        public void Build_Formula_With_Null_Base()
        {
            _formula = Formula.Get(null);

            TestFailedSymbolNotFound();
        }

        [TestMethod]
        public void Build_Formula_With_Zero_Base()
        {
            a.BuildZeroQuote();

            _formula = Formula.Get(_nodeBidA);

            TestSuccessfulWithZero();
        }

        [TestMethod]
        public void Build_Formula_With_One_Side_Base()
        {
            a.BuildOneSideAskQuote();

            _formula = Formula.Get(_nodeBidA);

            TestFailedOffQuotes();
        }

        [TestMethod]
        public void Build_Inverse_Formula()
        {
            _formula = Formula.Inv(_nodeBidA);

            var expected = 1.0 / _nodeBidA.Value;

            TestSuccessful(expected);
        }

        [TestMethod]
        public void Build_Inverse_Formula_With_Null_Base()
        {
            _formula = Formula.Inv(null);

            TestFailedSymbolNotFound();
        }

        [TestMethod]
        public void Build_Inverse_Formula_With_Zero_Base()
        {
            a.BuildZeroQuote();

            _formula = Formula.Inv(_nodeBidA);

            TestSuccessfulWithZero();
        }

        [TestMethod]
        public void Build_Inverse_Formula_With_One_Side_Base()
        {
            a.BuildOneSideAskQuote();

            _formula = Formula.Inv(_nodeBidA);

            TestFailedOffQuotes();
        }

        [TestMethod]
        public void FullFormula()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            CheckFullUpdate();
        }

        [TestMethod]
        public void FullFormula_Update()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            CheckFullUpdate();
            CheckFullUpdate();
            CheckFullUpdate();
        }

        [TestMethod]
        public void FullFormula_A_Base_Null()
        {
            _nodeBidA = new BidSideNode(null);

            _formula = Build_Inv_Mul_Div_Formula();

            TestFailedSymbolNotFound();
        }

        [TestMethod]
        public void FullFormula_A_Base_Zero()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            a.BuildZeroQuote();

            TestSuccessfulWithZero();
        }

        [TestMethod]
        public void FullFormula_A_Base_One_Side_Suc()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            a.BuildOneSideBidQuote();

            TestSuccessful(Actual_Inv_Mul_Div_Value);
        }

        [TestMethod]
        public void FullFormula_A_Base_One_Side_Failed()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            a.BuildOneSideAskQuote();

            TestFailedOffCrossQuotes();
        }

        [TestMethod]
        public void FullFormula_B_Base_Null()
        {
            _nodeBidB = new BidSideNode(null);

            _formula = Build_Inv_Mul_Div_Formula();

            TestFailedCrossSymbolNotFound();
        }

        [TestMethod]
        public void FullFormula_B_Base_Zero()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            b.BuildZeroQuote();

            TestSuccessfulWithZero();
        }

        [TestMethod]
        public void FullFormula_B_Base_One_Side_Suc()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            b.BuildOneSideBidQuote();

            TestSuccessful(Actual_Inv_Mul_Div_Value);
        }

        [TestMethod]
        public void FullFormula_B_Base_One_Side_Failed()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            b.BuildOneSideAskQuote();

            TestFailedOffCrossQuotes();
        }

        [TestMethod]
        public void FullFormula_C_Base_Null()
        {
            _nodeAskC = new AskSideNode(null);

            _formula = Build_Inv_Mul_Div_Formula();

            TestFailedCrossSymbolNotFound();
        }

        [TestMethod]
        public void FullFormula_C_Base_Zero()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            c.BuildZeroQuote();

            TestSuccessfulWithZero();
        }

        [TestMethod]
        public void FullFormula_C_Base_One_Side_Suc()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            c.BuildOneSideAskQuote();

            TestSuccessful(Actual_Inv_Mul_Div_Value);
        }

        [TestMethod]
        public void FullFormula_C_Base_One_Side_Failed()
        {
            _formula = Build_Inv_Mul_Div_Formula();

            c.BuildOneSideBidQuote();

            TestFailedOffCrossQuotes();
        }

        internal void CheckFullUpdate()
        {
            a.BuildNewQuote();
            b.BuildNewQuote();
            c.BuildNewQuote();

            TestSuccessful(Actual_Inv_Mul_Div_Value);
        }


        internal void TestSuccessful(double expected)
        {
            Assert.AreEqual(CalculationError.None, _formula.Error);
            Assert.AreEqual(expected, _formula.Value);
        }

        internal void TestSuccessfulWithZero()
        {
            Assert.AreEqual(CalculationError.None, _formula.Error);
            Assert.AreEqual(0.0, _formula.Value);
        }

        internal void TestFailedSymbolNotFound()
        {
            Assert.AreEqual(CalculationError.SymbolNotFound, _formula.Error);
            Assert.AreEqual(0.0, _formula.Value);
        }

        internal void TestFailedCrossSymbolNotFound()
        {
            Assert.AreEqual(CalculationError.NoCrossSymbol, _formula.Error);
            Assert.AreEqual(0.0, _formula.Value);
        }

        internal void TestFailedOffQuotes()
        {
            Assert.AreEqual(CalculationError.OffQuote, _formula.Error);
            Assert.AreEqual(0.0, _formula.Value);
        }

        internal void TestFailedOffCrossQuotes()
        {
            Assert.AreEqual(CalculationError.OffCrossQuote, _formula.Error);
            Assert.AreEqual(0.0, _formula.Value);
        }
    }
}
