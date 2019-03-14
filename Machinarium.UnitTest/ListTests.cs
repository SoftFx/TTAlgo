using Machinarium.Qnil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Machinarium.UnitTest
{
    [TestClass]
    public class ListTests
    {
        [TestMethod]
        public void List_Combine_AddRemove1()
        {
            var list1 = new VarList<int>();
            var list2 = new VarList<int>();

            var combo = VarCollection.CombineChained(list1, list2);

            var observable = combo.AsObservable();

            list1.Add(1);
            list1.Add(2);

            list2.Add(11);

            list1.Add(3);

            AssertEqual(observable.ToList(), 1, 2, 3, 11);

            list2.Remove(11);

            AssertEqual(observable.ToList(), 1, 2, 3);
        }

        [TestMethod]
        public void List_Combine_AddRemove2()
        {
            var list1 = new VarList<int>();
            var list2 = new VarList<int>();
            var list3 = new VarList<int>();

            var combined = VarCollection.Combine(list1, list2, list3);

            list1.Add(1);
            list1.Add(2);
            list3.Add(5);
            list3.Add(6);
            list1.Add(3);
            list3.Add(8);

            AssertEqual(combined.Snapshot, 1, 2, 3, 5, 6, 8);

            list3.Remove(6);

            AssertEqual(combined.Snapshot, 1, 2, 3, 5, 8);

            //list1.Add(3);

            //AssertEqual(combined.Snapshot, 1, 3, 5, 8);

            //list2.Add(4);

            //AssertEqual(combined.Snapshot, 1, 3, 4, 5, 8);

            //list3.Remove(8);

            //AssertEqual(combined.Snapshot, 1, 3, 4, 5);

            //list3.Add(7);

            //AssertEqual(combined.Snapshot, 1, 3, 4, 5, 7);
        }

        [TestMethod]
        public void List_OrderBy_Remove()
        {
            var src = new VarDictionary<int, string>();

            src.Add(1, "xxx");
            src.Add(2, "bbb");
            src.Add(3, "yyy");
            src.Add(4, "ccc");

            var selector = src.OrderBy((k, v) => v).Select(a => a);

            AssertEqual(selector.Snapshot, "bbb", "ccc", "xxx", "yyy");

            src.Remove(4); // used to throw out of range on src modification, due to incorrect init

            AssertEqual(selector.Snapshot, "bbb", "xxx", "yyy");

            src.Add(10, "aaa");

            AssertEqual(selector.Snapshot, "aaa", "bbb", "xxx", "yyy");
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
