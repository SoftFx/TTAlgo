using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class CollectionsExt
    {
        public static int IndexOf<T>(this IList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
                if (predicate(list[i]))
                    return i;
            return -1;
        }

        public static TVal Read<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dicionary, TKey key)
        {
            TVal value;
            dicionary.TryGetValue(key, out value);
            return value;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dicionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue val;
            if (!dicionary.TryGetValue(key, out val))
                return defaultValue;
            return val;
        }
    }
}
