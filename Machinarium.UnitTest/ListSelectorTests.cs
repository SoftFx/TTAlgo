using Machinarium.Qnil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Machinarium.UnitTest
{
    [TestClass]
    public class ListSelectorTests
    {
        [TestMethod]
        public void InitFailedTest()
        {
            var src = new DynamicDictionary<int, string>();

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
