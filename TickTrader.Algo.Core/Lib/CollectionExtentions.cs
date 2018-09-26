using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public static class CollectionExtentions
    {
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> collection, TKey key)
        {
            TValue result;
            collection.TryGetValue(key, out result);
            return result;
        }

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> collection, TKey key, TValue defaultValue)
        {
            TValue result;
            if (!collection.TryGetValue(key, out result))
                return defaultValue;
            return result;
        }
    }
}
