using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class CollectionExtentions
    {
        public static T GetOrDefault<K, T>(this IDictionary<K, T> dictionary, K key)
        {
            T result;
            dictionary.TryGetValue(key, out result);
            return result;
        }
    }
}
