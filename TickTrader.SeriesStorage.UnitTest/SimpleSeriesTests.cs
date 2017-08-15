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
    public class SimpleSeriesTests
    {
        [TestMethod]
        public void SimpleStorage_Iterate_ExactSlices()
        {
            string[] actual, expected;

            var series = Setup1();

            actual = series.Iterate(1, 16).Select(i => i.Value).ToArray();
            expected = new String[] { "two", "four", "seven", "eight", "nine", "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(8, 16).Select(i => i.Value).ToArray();
            expected = new String[] { "eight", "nine", "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(8, 10).Select(i => i.Value).ToArray();
            expected = new String[] { "eight", "nine" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(1, 10).Select(i => i.Value).ToArray();
            expected = new String[] { "two", "four", "seven", "eight", "nine" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(10, 16).Select(i => i.Value).ToArray();
            expected = new String[] { "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleStorage_Iterate_ExactKeys()
        {
            string[] actual, expected;

            var series = Setup1();

            actual = series.Iterate(2, 15).Select(i => i.Value).ToArray();
            expected = new String[] { "two", "four", "seven", "eight", "nine", "eleven" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(4, 11).Select(i => i.Value).ToArray();
            expected = new String[] { "four", "seven", "eight", "nine" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(8, 10).Select(i => i.Value).ToArray();
            expected = new String[] { "eight", "nine" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(8, 12).Select(i => i.Value).ToArray();
            expected = new String[] { "eight", "nine", "eleven" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleStorage_Iterate_NotExact()
        {
            string[] actual, expected;

            var series = Setup1();

            actual = series.Iterate(-1, 18).Select(i => i.Value).ToArray();
            expected = new String[] { "two", "four", "seven", "eight", "nine", "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(-1, 3).Select(i => i.Value).ToArray();
            expected = new String[] { "two" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(6, 10).Select(i => i.Value).ToArray();
            expected = new String[] { "seven", "eight", "nine" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(-1, 2).Select(i => i.Value).ToArray();
            expected = new String[] { };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(20, 45).Select(i => i.Value).ToArray();
            expected = new String[] { };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(-10, 0).Select(i => i.Value).ToArray();
            expected = new String[] { };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleStorage_Write_NoIntersect()
        {
            var series = Setup0();
            series.Write(1, 8, new MockItem(2, "two"), new MockItem(4, "four"), new MockItem(7, "seven"));
            series.Write(8, 10, new MockItem(8, "eight"), new MockItem(9, "nine"));
            series.Write(10, 16, new MockItem(11, "eleven"), new MockItem(15, "fifteen"));

            var actual = series.Iterate(1, 16).Select(i => i.Value).ToArray();
            var expected = new String[] { "two", "four", "seven", "eight", "nine", "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleStorage_Overwrite_Right_NoIntersect()
        {
            var series = Setup0();
            series.Write(1, 8, new MockItem(2, "two"), new MockItem(4, "four"), new MockItem(7, "seven"));
            series.Write(8, 10, new MockItem(8, "eight"), new MockItem(9, "nine"));
            series.Write(10, 16, new MockItem(11, "eleven"), new MockItem(15, "fifteen"));

            series.Write(8, 16, new MockItem(13, "thirteen"));

            var actual = series.Iterate(1, 15).Select(i => i.Value).ToArray();
            var expected = new String[] { "two", "four", "seven", "thirteen" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleStorage_Overwrite_Left_NoIntersect()
        {
            var series = Setup0();
            series.Write(1, 8, new MockItem(2, "two"), new MockItem(4, "four"), new MockItem(7, "seven"));
            series.Write(8, 10, new MockItem(8, "eight"), new MockItem(9, "nine"));
            series.Write(10, 16, new MockItem(11, "eleven"), new MockItem(15, "fifteen"));

            series.Write(0, 10, new MockItem(5, "five"));

            var actual = series.Iterate(1, 16).Select(i => i.Value).ToArray();
            var expected = new String[] { "five", "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleStorage_Overwrite_Left_Intersect1()
        {
            var series = Setup0();
            series.Write(1, 8, new MockItem(2, "two"), new MockItem(4, "four"), new MockItem(7, "seven"));
            series.Write(8, 10, new MockItem(8, "eight"), new MockItem(9, "nine"));
            series.Write(10, 16, new MockItem(11, "eleven"), new MockItem(15, "fifteen"));

            series.Write(3, 13, new MockItem(3, "three"), new MockItem(5, "five"), new MockItem(12, "twelve"));

            var actual = series.Iterate(1, 16).Select(i => i.Value).ToArray();
            var expected = new String[] { "two", "three", "five", "twelve", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleStorage_Overwrite_Left_Intersect2()
        {
            var series = Setup0();
            series.Write(1, 8, new MockItem(2, "two"), new MockItem(4, "four"), new MockItem(7, "seven"));
            series.Write(8, 10, new MockItem(8, "eight"), new MockItem(9, "nine"));
            series.Write(10, 16, new MockItem(11, "eleven"), new MockItem(15, "fifteen"));

            series.Write(3, 13, new MockItem(3, "three"), new MockItem(5, "five"), new MockItem(12, "twelve"));

            var actual = series.Iterate(1, 16).Select(i => i.Value).ToArray();
            var expected = new String[] { "two", "three", "five", "twelve", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);
        }

        private SeriesStorage<int, MockItem> Setup0()
        {
            var storage = new MockStorage<KeyRange<int>, MockItem[]>();
            return SeriesStorage.Create(storage, i => i.Id);
        }

        private SeriesStorage<int, MockItem> Setup1()
        {
            var storage = new MockStorage<KeyRange<int>, MockItem[]>();
            Write(storage, 1, 8, new MockItem(2, "two"), new MockItem(4, "four"), new MockItem(7, "seven"));
            Write(storage, 8, 10, new MockItem(8, "eight"), new MockItem(9, "nine"));
            Write(storage, 10, 16, new MockItem(11, "eleven"), new MockItem(15, "fifteen"));
            return SeriesStorage.Create(storage, i => i.Id);
        }

        private void Write(MockStorage<KeyRange<int>, MockItem[]> storage, int from, int to, params MockItem[] items)
        {
            storage.Write(new KeyRange<int>(from, to), items);
        }
    }
}
