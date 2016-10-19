using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMBot
{
    public static class Helper
    {
        public static TValue GetOrAdd<TKey,TValue>(this IDictionary<TKey, TValue> dic, TKey key, Func<TValue> valFactory)
        {
            TValue val;
            if (dic.TryGetValue(key, out val))
                return val;

            val = valFactory();
            dic[key] = val;
            return val;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
            where TValue : new()
        {
            TValue val;
            if (dic.TryGetValue(key, out val))
                return val;

            val = new TValue();
            dic[key] = val;
            return val;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            TValue val;
            if (dic.TryGetValue(key, out val))
                return val;
            return default(TValue);
        }
    }
}
