using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot2
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

        public static bool ExistByCurrencies(this SymbolList list, string baseCurrency, string counterCurrency)
        {
            return list.Any(s => s.BaseCurrency == baseCurrency && s.CounterCurrency == counterCurrency);
        }

        public static Symbol GetByCurrencies(this SymbolList list, string baseCurrency, string counterCurrency)
        {
            return list.FirstOrDefault(s => s.BaseCurrency == baseCurrency && s.CounterCurrency == counterCurrency);
        }
    }
}
