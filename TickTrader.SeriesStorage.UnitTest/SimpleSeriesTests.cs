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

            var series = new SimpleSeriesStorage<int, MockItem>(Setup1(), i => i.Id);

            actual = series.Iterate(1, 15).Select(i => i.Value).ToArray();
            expected = new String[] { "two", "four", "seven", "eight", "nine", "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(8, 15).Select(i => i.Value).ToArray();
            expected = new String[] { "eight", "nine", "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(8, 10).Select(i => i.Value).ToArray();
            expected = new String[] { "eight", "nine" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(1, 10).Select(i => i.Value).ToArray();
            expected = new String[] { "two", "four", "seven", "eight", "nine" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(10, 15).Select(i => i.Value).ToArray();
            expected = new String[] { "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleStorage_Iterate_ExactKeys()
        {
            string[] actual, expected;

            var series = new SimpleSeriesStorage<int, MockItem>(Setup1(), i => i.Id);

            actual = series.Iterate(2, 15).Select(i => i.Value).ToArray();
            expected = new String[] { "two", "four", "seven", "eight", "nine", "eleven", "fifteen" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(4, 11).Select(i => i.Value).ToArray();
            expected = new String[] { "four", "seven", "eight", "nine", "eleven" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(8, 9).Select(i => i.Value).ToArray();
            expected = new String[] { "eight", "nine" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(8, 11).Select(i => i.Value).ToArray();
            expected = new String[] { "eight", "nine", "eleven" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleStorage_Iterate_NotExact()
        {
            string[] actual, expected;

            var series = new SimpleSeriesStorage<int, MockItem>(Setup1(), i => i.Id);

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
            expected = new String[] { "two" };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(20, 45).Select(i => i.Value).ToArray();
            expected = new String[] { };
            CollectionAssert.AreEqual(expected, actual);

            actual = series.Iterate(-10, 0).Select(i => i.Value).ToArray();
            expected = new String[] { };
            CollectionAssert.AreEqual(expected, actual);
        }

        private MockStorage<int, MockItem> Setup1()
        {
            var storage = new MockStorage<int, MockItem>();
            storage.AddSlice(1, 8, new MockItem(2, "two"), new MockItem(4, "four"), new MockItem(7, "seven"));
            storage.AddSlice(8, 10, new MockItem(8, "eight"), new MockItem(9, "nine"));
            storage.AddSlice(10, 15, new MockItem(11, "eleven"), new MockItem(15, "fifteen"));
            return storage;
        }

        private class MockItem
        {
            public MockItem(int id, string val)
            {
                Id = id;
                Value = val;
            }

            public int Id { get; }
            public string Value { get; }

            public override string ToString()
            {
                return Value;
            }
        }
    }
}
