using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public static class KeyHelper
    {
        public static bool IsLess<TKey>(TKey key1, TKey key2)
            where TKey : IComparable
        {
            return key1.CompareTo(key2) < 0;
        }

        public static bool IsLessOrEqual<TKey>(TKey key1, TKey key2)
            where TKey : IComparable
        {
            return key1.CompareTo(key2) <= 0;
        }

        public static bool IsGreater<TKey>(TKey key1, TKey key2)
            where TKey : IComparable
        {
            return key1.CompareTo(key2) > 0;
        }

        public static bool IsGreaterOrEqual<TKey>(TKey key1, TKey key2)
            where TKey : IComparable
        {
            return key1.CompareTo(key2) >= 0;
        }

        public static bool IsEqual<TKey>(TKey key1, TKey key2)
            where TKey : IComparable
        {
            return key1.CompareTo(key2) == 0;
        }

        public static int Compare<TKey>(TKey key1, TKey key2)
            where TKey : IComparable
        {
            return key1.CompareTo(key2);
        }

        public static TKey Min<TKey>(TKey key1, TKey key2)
            where TKey : IComparable
        {
            return IsLess(key1, key2) ? key1 : key2;
        }

        public static TKey Max<TKey>(TKey key1, TKey key2)
            where TKey : IComparable
        {
            return IsLess(key1, key2) ? key2 : key1;
        }
    }
}
