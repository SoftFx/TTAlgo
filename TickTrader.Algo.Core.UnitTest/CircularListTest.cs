using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.UnitTest
{
    [TestClass]
    public class CircularListTest
    {
        [TestMethod]
        public void CircularList_Expand_Dequeue_Expand()
        {
            var list = new CircularList<int>();
            list.Add(10);
            list.Add(11);
            list.Add(12);
            list.Add(13);

            Assert.IsTrue(list.SequenceEqual(new int[] { 10, 11, 12, 13 }));
            Assert.AreEqual(10, list[0]);
            Assert.AreEqual(11, list[1]);
            Assert.AreEqual(12, list[2]);
            Assert.AreEqual(13, list[3]);

            Assert.AreEqual(10, list.Dequeue());
            Assert.AreEqual(11, list.Dequeue());
            Assert.AreEqual(12, list.Dequeue());

            list.Add(14);
            list.Add(15);
            list.Add(16);
            list.Add(17);
            list.Add(18);

            Assert.IsTrue(list.SequenceEqual(new int[] { 13, 14, 15, 16, 17, 18}));
            Assert.AreEqual(13, list[0]);
            Assert.AreEqual(14, list[1]);
            Assert.AreEqual(15, list[2]);
            Assert.AreEqual(16, list[3]);
            Assert.AreEqual(17, list[4]);
            Assert.AreEqual(18, list[5]);
        }

        [TestMethod]
        public void CircularList_Expand1Capacity()
        {
            var list = new CircularList<int>(1);
            list.Add(10);
            list.Add(11);

            Assert.IsTrue(list.SequenceEqual(new int[] { 10, 11 }));
            Assert.AreEqual(10, list[0]);
            Assert.AreEqual(11, list[1]);
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void CircularList_TruncateStart_Add_TruncateStart()
        {
            var list = new CircularList<int>();
            list.Add(10);
            list.Add(11);
            list.Add(12);
            list.Add(13);

            list.TruncateStart(2);

            Assert.IsTrue(list.SequenceEqual(new int[] { 12, 13 }));
            Assert.AreEqual(12, list[0]);
            Assert.AreEqual(13, list[1]);
            Assert.AreEqual(2, list.Count);

            list.Add(14);
            list.Add(15);

            Assert.IsTrue(list.SequenceEqual(new int[] { 12, 13, 14, 15 }));
            Assert.AreEqual(12, list[0]);
            Assert.AreEqual(13, list[1]);
            Assert.AreEqual(14, list[2]);
            Assert.AreEqual(15, list[3]);
            Assert.AreEqual(4, list.Count);

            list.TruncateStart(3);

            Assert.IsTrue(list.SequenceEqual(new int[] { 15 }));
            Assert.AreEqual(15, list[0]);
            Assert.AreEqual(1, list.Count);
        }
    }
}
