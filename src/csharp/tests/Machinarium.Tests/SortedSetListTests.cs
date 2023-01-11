using Machinarium.Qnil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Machinarium.Tests
{
    [TestClass]
    public class SortedSetListTests
    {
        [TestMethod]
        public void BasicAddRemove()
        {
            var list = new SortedVarSetList<int>();

            list.Add(15);
            list.Add(5);

            AssertEqual(list.Values, 5, 15);

            list.Add(4);
            list.Add(15);

            AssertEqual(list.Values, 4, 5, 15);

            list.Add(8);
            AssertEqual(list.Values, 4, 5, 8, 15);

            list.Remove(8);
            list.Add(5);
            list.Remove(5);
            list.Add(8);

            AssertEqual(list.Values, 4, 8, 15);

            list.Add(1);
            list.Add(5);
            list.RemoveAt(3);

            AssertEqual(list.Values, 1, 4, 5, 15);

            list.RemoveAt(0);
            list.RemoveAt(0);
            list.RemoveAt(1);

            AssertEqual(list.Values, 5);
        }


        [TestMethod]
        public void CompositeAddRemove()
        {
            var list = new SortedVarSetList<Item>(new ItemKeyComparer());
            var listView = list.Select(v => v.Value);

            list.Add(new Item(6, 12));
            list.Add(new Item(3, 6));
            list.Add(new Item(4, 8));
            list.Add(new Item(7, 14));
            list.Add(new Item(1, 2));

            AssertEqual(listView.Snapshot, 2, 6, 8, 12, 14);

            list.Remove(new Item(6, -1)); AssertEqual(listView.Snapshot, 2, 6, 8, 14);
            list.Remove(new Item(3, -1)); AssertEqual(listView.Snapshot, 2, 8, 14);
            list.Remove(new Item(7, -1)); AssertEqual(listView.Snapshot, 2, 8);

            for (var i = 1; i < 11; i++) list.Add(new Item(i, 2 * i));
            AssertEqual(listView.Snapshot, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20);

            list.RemoveAt(0); list.RemoveAt(2); list.RemoveAt(7); list.RemoveAt(5);
            AssertEqual(listView.Snapshot, 4, 6, 10, 12, 14, 18);

            for (var i = 1; i < 11; i++) list.Add(new Item(i, 2 * i));
            AssertEqual(listView.Snapshot, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20);

            list.Clear();
            AssertEqual(listView.Snapshot, Enumerable.Empty<int>());
        }

        private record Item(int Key, int Value);

        private class ItemKeyComparer : IComparer<Item>
        {
            public int Compare(Item x, Item y) => x.Key.CompareTo(y.Key);
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
