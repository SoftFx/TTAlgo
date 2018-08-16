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

        #region Binary Keys

        public static bool IsLess(byte[] key1, byte[] key2)
        {
            return Compare(key1, key2) < 0;
        }

        public static bool IsLessOrEqual(byte[] key1, byte[] key2)
        {
            return Compare(key1, key2) <= 0;
        }

        public static bool IsGreater(byte[] key1, byte[] key2)
        {
            return Compare(key1, key2) > 0;
        }

        public static bool IsGreaterOrEqual(byte[] key1, byte[] key2)
        {
            return Compare(key1, key2) >= 0;
        }

        public static bool IsInRange(byte[] key, byte[] from, byte[] to)
        {
            return IsGreaterOrEqual(key, from) && IsLessOrEqual(key, to);
        }

        public static int Compare(byte[] key1, byte[] key2)
        {
            if (key1.Length != key2.Length)
                return key1.Length.CompareTo(key2.Length);

            for (int i = 0; i < key1.Length; i++)
            {
                var cmp = key1[i].CompareTo(key2[i]);
                if (cmp != 0)
                    return cmp;
            }

            return 0;
        }


        #endregion
    }
}
