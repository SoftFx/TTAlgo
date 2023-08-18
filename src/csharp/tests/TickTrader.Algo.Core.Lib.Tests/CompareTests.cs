using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Core.Lib.Tests
{
    [TestClass]
    public class CompareTests
    {
        [TestMethod]
        public void SimpleTests()
        {
            AssertE(1.5, 1.5);
            AssertLte(1.5, 1.5);
            AssertGte(1.5, 1.5);
            AssertE(-1.5, -1.5);
            AssertLte(-1.5, -1.5);
            AssertGte(-1.5, -1.5);

            AssertLt(1.5, 1.6);
            AssertLte(1.5, 1.6);
            AssertLt(-1.5, -1.4);
            AssertLte(-1.5,- 1.4);

            AssertGt(1.5, 1.4);
            AssertGte(1.5, 1.4);
            AssertGt(-1.5, -1.6);
            AssertGte(-1.5,- 1.6);

            AssertE0(1e-15);
            AssertE0(-1e-15);
        }


        private static void AssertE0(double a) => Assert.IsTrue(a.E0());

        private static void AssertE(double a, double b) => Assert.IsTrue(a.E(b));

        private static void AssertLt(double a, double b) => Assert.IsTrue(a.Lt(b));

        private static void AssertLte(double a, double b) => Assert.IsTrue(a.Lte(b));

        private static void AssertGt(double a, double b) => Assert.IsTrue(a.Gt(b));

        private static void AssertGte(double a, double b) => Assert.IsTrue(a.Gte(b));
    }
}
