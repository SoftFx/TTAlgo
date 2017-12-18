using Machinarium.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Machinarium.UnitTest
{
    [TestClass]
    public class SortedListTests
    {
        [TestMethod]
        public void SortedListTest1()
        {
            var list = new SortedList<int>();

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
