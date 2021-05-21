using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Core.Lib
{
    public static class CollectionExtentions
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> collection, TKey key)
        {
            collection.TryGetValue(key, out TValue result);
            return result;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> collection, TKey key, TValue defaultValue)
        {
            if (!collection.TryGetValue(key, out TValue result))
                return defaultValue;
            return result;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> collection, TKey key, Func<TValue> valueProvider)
        {
            if (!collection.TryGetValue(key, out TValue val))
            {
                val = valueProvider();
                collection.Add(key, val);
            }
            return val;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> collection, TKey key, Func<TKey, TValue> valueProvider)
        {
            if (!collection.TryGetValue(key, out TValue val))
            {
                val = valueProvider(key);
                collection.Add(key, val);
            }
            return val;
        }

        public static void RemoveLast<T>(this IList<T> collection)
        {
            collection.RemoveAt(collection.Count - 1);
        }
    }
}
