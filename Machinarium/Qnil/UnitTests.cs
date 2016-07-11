using Machinarium.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
#if DEBUG

    public static class UnitTests
    {
        public static void Launch()
        {
            SortedListTest1();
            DictionaryOrderByTest();
            DictionaryStaticCompositionTest();
            DictionaryWhereAndSelectTest();
            DictionaryGroupingTest();
        }

        private static void SortedListTest1()
        {
            SortedList<int> list = new SortedList<int>();

            list.Add(5);
            list.Add(4);
            list.Add(15);

            AssertEqual(list, 4, 5, 15);

            list.Add(8);
            list.Remove(5);

            AssertEqual(list, 4, 8, 15);

            list.Add(1);
            list.Remove(15);

            AssertEqual(list, 1, 4, 8);

            list.RemoveAt(2);
            list.RemoveAt(0);

            AssertEqual(list, 4);
        }

        private static void DictionaryOrderByTest()
        {
            DynamicDictionary<string, int> src = new DynamicDictionary<string, int>();

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

        private static void DictionarySelectAndFilterTest()
        {
        }

        private static void DictionaryStaticCompositionTest()
        {
            DynamicDictionary<string, int> src1 = new DynamicDictionary<string, int>();
            DynamicDictionary<string, int> src2 = new DynamicDictionary<string, int>();

            src1.Add("key1", 1);
            src1.Add("key2", 2);

            src2.Add("key3", 3);
            src2.Add("key4", 4);

            var composition = Dynamic.Combine(src1, src2);

            Trace.Assert(composition.Snapshot.Count == 4);
            Trace.Assert(composition.Snapshot["key1"] == 1);
            Trace.Assert(composition.Snapshot["key2"] == 2);
            Trace.Assert(composition.Snapshot["key3"] == 3);
            Trace.Assert(composition.Snapshot["key4"] == 4);

            src1.Remove("key1");
            src1.Add("key5", 5);

            Trace.Assert(composition.Snapshot.Count == 4);
            Trace.Assert(composition.Snapshot["key2"] == 2);
            Trace.Assert(composition.Snapshot["key3"] == 3);
            Trace.Assert(composition.Snapshot["key4"] == 4);
            Trace.Assert(composition.Snapshot["key5"] == 5);

            src1.Clear();
            src2["key4"] = 8;

            Trace.Assert(composition.Snapshot.Count == 2);
            Trace.Assert(composition.Snapshot["key3"] == 3);
            Trace.Assert(composition.Snapshot["key4"] == 8);

        }

        private static class TestItemWithDictionary
        {
            //public DynamicDictionary<string, 
        }

        private static void DictionaryWhereAndSelectTest()
        {
            DynamicDictionary<string, int> src = new DynamicDictionary<string, int>();

            src.Add("key1", 5);
            src.Add("key2", 4);

            var doubled = src.Select((k, v) => v * 2);
            var doubledAndGreater9 = doubled.Where((k, v) => v > 9);

            Trace.Assert(doubled.Snapshot.Count == 2);
            Trace.Assert(doubled.Snapshot["key1"] == 10);
            Trace.Assert(doubled.Snapshot["key2"] == 8);

            Trace.Assert(doubledAndGreater9.Snapshot.Count == 1);
            Trace.Assert(doubledAndGreater9.Snapshot["key1"] == 10);

            src.Add("key3", 11);
            src.Remove("key2");

            Trace.Assert(doubled.Snapshot.Count == 2);
            Trace.Assert(doubled.Snapshot["key1"] == 10);
            Trace.Assert(doubled.Snapshot["key3"] == 22);

            Trace.Assert(doubledAndGreater9.Snapshot.Count == 2);
            Trace.Assert(doubledAndGreater9.Snapshot["key1"] == 10);
            Trace.Assert(doubledAndGreater9.Snapshot["key3"] == 22);

            src.Add("key5", 2);
            src.Remove("key3");

            Trace.Assert(doubled.Snapshot.Count == 2);
            Trace.Assert(doubled.Snapshot["key1"] == 10);
            Trace.Assert(doubled.Snapshot["key5"] == 4);

            Trace.Assert(doubledAndGreater9.Snapshot.Count == 1);
            Trace.Assert(doubledAndGreater9.Snapshot["key1"] == 10);

            src["key5"] = 11;

            Trace.Assert(doubled.Snapshot.Count == 2);
            Trace.Assert(doubled.Snapshot["key1"] == 10);
            Trace.Assert(doubled.Snapshot["key5"] == 22);

            Trace.Assert(doubledAndGreater9.Snapshot.Count == 2);
            Trace.Assert(doubledAndGreater9.Snapshot["key1"] == 10);
            Trace.Assert(doubledAndGreater9.Snapshot["key5"] == 22);
        }

        private static void DictionaryGroupingTest()
        {
            var src = new DynamicDictionary<string, int>();

            src.Add("key1", 1);
            src.Add("key2", 1);
            src.Add("key3", 2);

            var groupByVal = src.GroupBy((k, v) => v);

            Trace.Assert(groupByVal.Snapshot.Count == 2);
            Trace.Assert(groupByVal.Snapshot[1].Snapshot["key1"] == 1);
            Trace.Assert(groupByVal.Snapshot[1].Snapshot["key2"] == 1);
            Trace.Assert(groupByVal.Snapshot[2].Snapshot["key3"] == 2);
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
                Trace.Fail("Collections are not equal!");

            for (int i = 0; i < list1.Count; i++)
            {
                if (!comparer.Equals(list1[i], list2[i]))
                    Trace.Fail("Collections are not equal!");
            }
        }
    }

#endif
}
