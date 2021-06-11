using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestEnviroment;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator.Tests.FormulaBuilderTests
{
    [TestClass]
    public class FormulaBuilderTests
    {
        private static SymbolInfoStorage _storage = SymbolInfoStorage.Instance;
        private static SymbolInfo a, b, c;

        private ISideNode _nodeA, _nodeB, _nodeC;


        [ClassInitialize]
        public static void InitClass(TestContext _)
        {
            a = _storage.Symbols["EURUSD"];
            b = _storage.Symbols["EURAUD"];
            c = _storage.Symbols["AUDUSD"];
        }

        [TestInitialize]
        public void LoadSymbols()
        {
            _nodeA = new BidSideNode(a.BuildNewQuote());
            _nodeB = new BidSideNode(b.BuildNewQuote());
            _nodeC = new AskSideNode(c.BuildNewQuote());
        }

        internal FormulaBuilder Build_Inv_Mul_Div_Formula() => Formula.Inv(_nodeA).Mul(_nodeB).Div(_nodeC);

        internal double Actual_Inv_Mul_Div_Value => 1.0 / _nodeA.Value * _nodeB.Value / _nodeC.Value;


        [TestMethod]
        public void Direct_Call()
        {
            var direct = Formula.Direct;

            Assert.AreEqual(direct.Value, 1.0);
            Assert.AreEqual(direct.ErrorCode, CalcErrorCodes.None);
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
            var error = Formula.ErrorBuild;

            Assert.AreEqual(error.Value, 0.0);
            Assert.AreEqual(error.ErrorCode, CalcErrorCodes.NoCrossSymbol);
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
            var formula = Formula.Get(_nodeA);

            var actual = formula.Value;
            var expected = _nodeA.Value;

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Build_Formula_With_Null_Base()
        {
            var formula = Formula.Get(null);

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void Build_Formula_With_Zero_Base()
        {
            a.BuildZeroQuote();

            var formula = Formula.Get(_nodeA);

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void Build_Formula_With_One_Side_Base()
        {
            a.BuildOneSideAskQuote();

            var formula = Formula.Get(_nodeA);

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void Build_Inverse_Formula()
        {
            var formula = Formula.Inv(_nodeA);

            var actual = formula.Value;
            var expected = 1.0 / _nodeA.Value;

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Build_Inverse_Formula_With_Null_Base()
        {
            var formula = Formula.Inv(null);

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void Build_Inverse_Formula_With_Zero_Base()
        {
            a.BuildZeroQuote();

            var formula = Formula.Inv(_nodeA);

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void Build_Inverse_Formula_With_One_Side_Base()
        {
            a.BuildOneSideAskQuote();

            var formula = Formula.Inv(_nodeA);

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void FullFormula()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            CheckFullUpdate(formula);
        }

        [TestMethod]
        public void FullFormula_Update()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            CheckFullUpdate(formula);
            CheckFullUpdate(formula);
            CheckFullUpdate(formula);
        }

        [TestMethod]
        public void FullFormula_A_Base_Null()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            a.BuildNullQuote();

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void FullFormula_A_Base_Zero()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            a.BuildZeroQuote();

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void FullFormula_A_Base_One_Side()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            a.BuildOneSideAskQuote();

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void FullFormula_B_Base_Null()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            b.BuildNullQuote();

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void FullFormula_B_Base_Zero()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            b.BuildZeroQuote();

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void FullFormula_B_Base_One_Side()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            b.BuildOneSideAskQuote();

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void FullFormula_C_Base_Null()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            c.BuildNullQuote();

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void FullFormula_C_Base_Zero()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            c.BuildZeroQuote();

            WaitingForZero(formula.Value);
        }

        [TestMethod]
        public void FullFormula_C_Base_One_Side()
        {
            var formula = Build_Inv_Mul_Div_Formula();

            c.BuildOneSideBidQuote();

            WaitingForZero(formula.Value);
        }

        internal void CheckFullUpdate(FormulaBuilder formula)
        {
            a.BuildNewQuote();
            b.BuildNewQuote();
            c.BuildNewQuote();

            Assert.AreEqual(formula.Value, Actual_Inv_Mul_Div_Value);
        }

        protected static void WaitingForZero(double actual) => Assert.AreEqual(actual, 0.0);
    }
}
