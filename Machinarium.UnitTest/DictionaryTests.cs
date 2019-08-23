using Machinarium.Qnil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Machinarium.UnitTest
{
    [TestClass]
    public class DictionaryTests
    {
        [TestMethod]
        public void Dictionary_Order_AddRemove()
        {
            var set = new VarDictionary<string, string>();
            var list = set.OrderBy((k, v) => k);

            set.Add("1", "one");
            set.Add("2", "two");
            set.Remove("1");
            set.Add("3", "three");
            set.Remove("3");
            set.Add("4", "four");

            Assert.AreEqual(2, list.Snapshot.Count);
            Assert.AreEqual("two", list.Snapshot[0]);
            Assert.AreEqual("four", list.Snapshot[1]);
        }

        [TestMethod]
        public void Dictionary_OrderBy()
        {
            var src = new VarDictionary<string, int>();

            src.Add("key1", 5);
            src.Add("key2", 4);

            var srotedByVal = src.OrderBy((k, v) => v);
            var srotedByKey = src.OrderBy((k, v) => k);

            AssertEqual(srotedByVal.Snapshot, 4, 5);
            AssertEqual(srotedByKey.Snapshot, 5, 4);

            src.Add("key3", 5);
            src.Add("key4", 5);

            AssertEqual(srotedByVal.Snapshot, 4, 5, 5, 5);
            AssertEqual(srotedByKey.Snapshot, 5, 4, 5, 5);

            src.Remove("key3");
            src.Remove("key1");

            AssertEqual(srotedByVal.Snapshot, 4, 5);
            AssertEqual(srotedByKey.Snapshot, 4, 5);

            src["key2"] = 6;

            AssertEqual(srotedByVal.Snapshot, 5, 6);
            AssertEqual(srotedByKey.Snapshot, 6, 5);

            src.Clear();

            AssertEqual(srotedByVal.Snapshot);
            AssertEqual(srotedByKey.Snapshot);
        }

        [TestMethod]
        public void Dictionary_Composition_AddRemove()
        {
            var src1 = new VarDictionary<string, int>();
            var src2 = new VarDictionary<string, int>();

            src1.Add("key1", 1);
            src1.Add("key2", 2);

            src2.Add("key3", 3);
            src2.Add("key4", 4);

            var composition = VarCollection.Combine(src1, src2);

            Assert.AreEqual(4, composition.Snapshot.Count);
            Assert.AreEqual(1, composition.Snapshot["key1"]);
            Assert.AreEqual(2, composition.Snapshot["key2"]);
            Assert.AreEqual(3, composition.Snapshot["key3"]);
            Assert.AreEqual(4, composition.Snapshot["key4"]);

            src1.Remove("key1");
            src1.Add("key5", 5);

            Assert.AreEqual(4, composition.Snapshot.Count);
            Assert.AreEqual(2, composition.Snapshot["key2"]);
            Assert.AreEqual(3, composition.Snapshot["key3"]);
            Assert.AreEqual(4, composition.Snapshot["key4"]);
            Assert.AreEqual(5, composition.Snapshot["key5"]);

            src1.Clear();
            src2["key4"] = 8;

            Assert.AreEqual(2, composition.Snapshot.Count);
            Assert.AreEqual(3, composition.Snapshot["key3"]);
            Assert.AreEqual(8, composition.Snapshot["key4"]);

        }

        private class TestItemWithDictionary
        {
            //public DynamicDictionary<string, 
        }

        [TestMethod]
        public void DictionaryWhereAndSelectTest()
        {
            var src = new VarDictionary<string, int>();

            src.Add("key1", 5);
            src.Add("key2", 4);

            var doubled = src.Select((k, v) => v * 2);
            var doubledAndGreater9 = doubled.Where((k, v) => v > 9);

            Assert.AreEqual(2, doubled.Snapshot.Count);
            Assert.AreEqual(10, doubled.Snapshot["key1"]);
            Assert.AreEqual(8, doubled.Snapshot["key2"]);

            Assert.AreEqual(1, doubledAndGreater9.Snapshot.Count);
            Assert.AreEqual(10, doubledAndGreater9.Snapshot["key1"]);

            src.Add("key3", 11);
            src.Remove("key2");

            Assert.AreEqual(2, doubled.Snapshot.Count);
            Assert.AreEqual(10, doubled.Snapshot["key1"]);
            Assert.AreEqual(22, doubled.Snapshot["key3"]);

            Assert.AreEqual(2, doubledAndGreater9.Snapshot.Count);
            Assert.AreEqual(10, doubledAndGreater9.Snapshot["key1"]);
            Assert.AreEqual(22, doubledAndGreater9.Snapshot["key3"]);

            src.Add("key5", 2);
            src.Remove("key3");

            Assert.AreEqual(2, doubled.Snapshot.Count);
            Assert.AreEqual(10, doubled.Snapshot["key1"]);
            Assert.AreEqual(4, doubled.Snapshot["key5"]);

            Assert.AreEqual(1, doubledAndGreater9.Snapshot.Count);
            Assert.AreEqual(10, doubledAndGreater9.Snapshot["key1"]);

            src["key5"] = 11;

            Assert.AreEqual(2, doubled.Snapshot.Count);
            Assert.AreEqual(10, doubled.Snapshot["key1"]);
            Assert.AreEqual(22, doubled.Snapshot["key5"]);

            Assert.AreEqual(2, doubledAndGreater9.Snapshot.Count);
            Assert.AreEqual(10, doubledAndGreater9.Snapshot["key1"]);
            Assert.AreEqual(22, doubledAndGreater9.Snapshot["key5"]);
        }

        [TestMethod]
        public void DictionaryGroupingTest()
        {
            var src = new VarDictionary<string, int>();

            src.Add("key1", 1);
            src.Add("key2", 1);
            src.Add("key3", 2);

            var groupByVal = src.GroupBy((k, v) => v);

            Assert.AreEqual(2, groupByVal.Snapshot.Count);
            Assert.AreEqual(1, groupByVal.Snapshot[1].Snapshot["key1"]);
            Assert.AreEqual(1, groupByVal.Snapshot[1].Snapshot["key2"]);
            Assert.AreEqual(2, groupByVal.Snapshot[2].Snapshot["key3"]);
        }


        private static void AssertEqual<T>(IEnumerable<T> s1, params T[] s2)
        {
            AssertEqual(s1, (IEnumerable<T>)s2);
        }

        private static void AssertEqual<T>(IEnumerable<T> s1, IEnumerable<T> s2)
        {
            var comparer = EqualityComparer<T>.Default;

            var list1 = s1.ToList();
            var list2 = s2.ToList();

            if (s1.Count() != s2.Count())
                throw new AssertFailedException("Collections are not equal!");

            for (var i = 0; i < list1.Count; i++)
            {
                if (!comparer.Equals(list1[i], list2[i]))
                    throw new AssertFailedException("Collections are not equal!");
            }
        }
    }
}
