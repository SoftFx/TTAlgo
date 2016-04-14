using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Metadata
{
    public static class EmptyValue
    {
        private static Dictionary<Type, object> predefinedValues = new Dictionary<Type, object>();

        static EmptyValue()
        {
            predefinedValues.Add(typeof(float), float.NaN);
            predefinedValues.Add(typeof(double), double.NaN);
            predefinedValues.Add(typeof(Api.Bar), BarEntity.Empty);
        }

        public static T Get<T>()
        {
            object predefined;
            if (predefinedValues.TryGetValue(typeof(T), out predefined))
                return (T)predefined;
            return default(T);
        }
    }
}
