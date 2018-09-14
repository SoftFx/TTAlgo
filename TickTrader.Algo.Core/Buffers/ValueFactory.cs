using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Entities;

namespace TickTrader.Algo.Core
{
    internal sealed class ValueFactory
    {
        private static Dictionary<Type, object> predefinedValues = new Dictionary<Type, object>();

        static ValueFactory()
        {
            Add(() => float.NaN);
            Add(() => double.NaN);
            Add(() => DateTime.MinValue);
            Add<Api.Bar>(() => BarEntity.Empty);
            Add<Api.Marker>(() => new MarkerEntity());
        }

        private static void Add<T>(Func<T> factoryFunc)
        {
            predefinedValues.Add(typeof(T), new ValueFactory<T>(factoryFunc));
        }

        public static ValueFactory<T> Get<T>()
        {
            object predefined;
            if (predefinedValues.TryGetValue(typeof(T), out predefined))
                return (ValueFactory<T>)predefined;
            return new ValueFactory<T>(() => default(T));
        }
    }

    internal class ValueFactory<T>
    {
        private Func<T> newValueFactory;
        private Func<T> nullValueFactory;

        public ValueFactory(Func<T> valueFactory)
            : this(valueFactory, valueFactory)
        {
        }

        public ValueFactory(Func<T> newValueFactory, Func<T> nullValueFactory)
        {
            this.newValueFactory = newValueFactory;
            this.nullValueFactory = nullValueFactory;
        }

        public T GetNullValue()
        {
            return newValueFactory();
        }

        public T GetNewValue()
        {
            return nullValueFactory();
        }
    }
}
