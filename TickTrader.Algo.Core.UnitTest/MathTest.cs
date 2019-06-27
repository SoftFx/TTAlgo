using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TickTrader.Algo.Core.UnitTest
{
    [TestClass]
    public class MathTest
    {
        [TestMethod]
        public void TestFloorBy_d0()
        {
            var decimals = 0;

            Assert.AreEqual(0, Rounding.FloorBy(0.25, decimals));
            Assert.AreEqual(-1, Rounding.FloorBy(-0.37, decimals));
            Assert.AreEqual(1, Rounding.FloorBy(1, decimals));
            Assert.AreEqual(1, Rounding.FloorBy(1.2, decimals));
            Assert.AreEqual(1, Rounding.FloorBy(1.99, decimals));
            Assert.AreEqual(1, Rounding.FloorBy(1.9999999999999, decimals));
            Assert.AreEqual(1234, Rounding.FloorBy(1234.5, decimals));
            Assert.AreEqual(-1, Rounding.FloorBy(-1, decimals));
            Assert.AreEqual(-2, Rounding.FloorBy(-1.9, decimals));
            Assert.AreEqual(-2, Rounding.FloorBy(-1.9999999999999, decimals));
        }

        [TestMethod]
        public void TestFloorBy_d1()
        {
            var decimals = 1;

            Assert.AreEqual(0.2, Rounding.FloorBy(0.25, decimals));
            Assert.AreEqual(-0.4, Rounding.FloorBy(-0.37, decimals));
            Assert.AreEqual(1, Rounding.FloorBy(1, decimals));
            Assert.AreEqual(1.2, Rounding.FloorBy(1.2, decimals));
            Assert.AreEqual(1.9, Rounding.FloorBy(1.99, decimals));
            Assert.AreEqual(1.9, Rounding.FloorBy(1.9999999999999, decimals));
            Assert.AreEqual(1234.5, Rounding.FloorBy(1234.5, decimals));
            Assert.AreEqual(-1, Rounding.FloorBy(-1, decimals));
            Assert.AreEqual(-1.9, Rounding.FloorBy(-1.9, decimals));
            Assert.AreEqual(-2, Rounding.FloorBy(-1.9999999999999, decimals));
        }

        [TestMethod]
        public void TestCeilBy_d0()
        {
            var decimals = 0;

            Assert.AreEqual(1, Rounding.CeilBy(0.25, decimals));
            Assert.AreEqual(0, Rounding.CeilBy(-0.37, decimals));
            Assert.AreEqual(1, Rounding.CeilBy(1, decimals));
            Assert.AreEqual(2, Rounding.CeilBy(1.2, decimals));
            Assert.AreEqual(2, Rounding.CeilBy(1.99, decimals));
            Assert.AreEqual(2, Rounding.CeilBy(1.9999999999999, decimals));
            Assert.AreEqual(1235, Rounding.CeilBy(1234.5, decimals));
            Assert.AreEqual(-1, Rounding.CeilBy(-1, decimals));
            Assert.AreEqual(-1, Rounding.CeilBy(-1.9, decimals));
            Assert.AreEqual(-1, Rounding.CeilBy(-1.9999999999999, decimals));
        }

        [TestMethod]
        public void TestCeilBy_d1()
        {
            var decimals = 1;

            Assert.AreEqual(0.3, Rounding.CeilBy(0.25, decimals));
            Assert.AreEqual(-0.3, Rounding.CeilBy(-0.37, decimals));
            Assert.AreEqual(1, Rounding.CeilBy(1, decimals));
            Assert.AreEqual(1.2, Rounding.CeilBy(1.2, decimals));
            Assert.AreEqual(2, Rounding.CeilBy(1.99, decimals));
            Assert.AreEqual(2, Rounding.CeilBy(1.9999999999999, decimals));
            Assert.AreEqual(3.3, Rounding.CeilBy(3.246, decimals));
            Assert.AreEqual(1234.5, Rounding.CeilBy(1234.5, decimals));
            Assert.AreEqual(-1, Rounding.CeilBy(-1, decimals));
            Assert.AreEqual(-1.9, Rounding.CeilBy(-1.9, decimals));
            Assert.AreEqual(-1.9, Rounding.CeilBy(-1.9999999999999, decimals));
        }
    }
}
