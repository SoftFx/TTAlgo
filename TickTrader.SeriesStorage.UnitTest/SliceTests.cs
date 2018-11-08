using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.SeriesStorage.UnitTest.Mocks;

namespace TickTrader.SeriesStorage.UnitTest
{
    [TestClass]
    public class SliceTests
    {
        private const bool checkFlags = false;

        [TestMethod]
        public void SliceTest_Check_Positive()
        {
            var slice = CreateSlice(1, 10, new MockItem(1, "one"), new MockItem(2, "two"), new MockItem(3, "three"));
            slice.Check(checkFlags);
        }

        [TestMethod]
        public void SliceTest_Check_LeftBorder()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var slice = CreateSlice(2, 10, new MockItem(1, "one"), new MockItem(2, "two"), new MockItem(3, "three"));
                slice.Check(checkFlags);
            });
        }

        [TestMethod]
        public void SliceTest_Check_RightBorder()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var slice = CreateSlice(0, 2, new MockItem(1, "one"), new MockItem(2, "two"), new MockItem(3, "three"));
                slice.Check(checkFlags);
            });
        }

        [TestMethod]
        public void SliceTest_Check_RightBorder_Exact()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var slice = CreateSlice(2, 3, new MockItem(1, "one"), new MockItem(2, "two"), new MockItem(3, "three"));
                slice.Check(checkFlags);
            });
        }

        [TestMethod]
        public void SliceTest_GetSegment_SameBounds()
        {
            var slice = CreateSlice(1, 10, new MockItem(1, "one"), new MockItem(2, "two"), new MockItem(3, "three"));
            var result = slice.GetSegment(1, 10);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { "one", "two", "three" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_Start_1()
        {
            var slice = CreateSlice(1, 10, new MockItem(1, "one"), new MockItem(2, "two"), new MockItem(3, "three"));
            var result = slice.GetSegment(1, 2);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { "one" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_Start_2()
        {
            var slice = CreateSlice(0, 10, new MockItem(1, "one"), new MockItem(2, "two"), new MockItem(3, "three"));
            var result = slice.GetSegment(0, 2);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { "one" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_Start_Empty_1()
        {
            var slice = CreateSlice(0, 10, new MockItem(3, "three"), new MockItem(4, "four"));
            var result = slice.GetSegment(1, 2);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_Start_Empty_2()
        {
            var slice = CreateSlice(0, 10, new MockItem(3, "three"), new MockItem(4, "four"));
            var result = slice.GetSegment(1, 3);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_Middle_Exact()
        {
            var slice = CreateSlice(1, 10, new MockItem(1, "one"), new MockItem(2, "two"), new MockItem(3, "three"), new MockItem(4, "four"));
            var result = slice.GetSegment(2, 4);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { "two", "three" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_Middle_NotExact()
        {
            var slice = CreateSlice(1, 10, new MockItem(1, "one"), new MockItem(3, "three"), new MockItem(4, "four"), new MockItem(5, "five"), new MockItem(7, "seven"));
            var result = slice.GetSegment(2, 6);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { "three", "four", "five" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_End_1()
        {
            var slice = CreateSlice(1, 10, new MockItem(1, "one"), new MockItem(2, "two"), new MockItem(3, "three"), new MockItem(9, "nine"));
            var result = slice.GetSegment(3, 10);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { "three", "nine" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_End_2()
        {
            var slice = CreateSlice(1, 10, new MockItem(1, "one"), new MockItem(3, "three"), new MockItem(7, "seven"));
            var result = slice.GetSegment(2, 9);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { "three", "seven" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_End_Empty_1()
        {
            var slice = CreateSlice(1, 10, new MockItem(3, "three"), new MockItem(4, "four"));
            var result = slice.GetSegment(5, 10);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SliceTest_GetSegment_End_Empty_2()
        {
            var slice = CreateSlice(1, 10, new MockItem(3, "three"), new MockItem(4, "four"));
            var result = slice.GetSegment(5, 8);
            var actual = result.Content.Select(i => i.Value).ToList();
            var expected = new String[] { };
            CollectionAssert.AreEqual(expected, actual);
        }

        private Slice<int, MockItem> CreateSlice(int from, int to, params MockItem[] items)
        {
            return Slice<int, MockItem>.Create(from, to, i => i.Id, items);
        }
    }
}
